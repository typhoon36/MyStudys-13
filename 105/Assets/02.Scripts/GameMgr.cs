using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    GS_Ready = 0,
    GS_Playing,
    GS_GameEnd
}

public class GameMgr : MonoBehaviourPunCallbacks
{
    public static bool isFocus = true;
    //�� Ŭ���̾�Ʈ �����찡 ���콺 ��Ŀ���� ������ �ִ���? Ȯ���ϴ� ����

    //���ӵ� �ÿ��̾� ���� ǥ���� Text UI �׸� ����
    public Text txtConnect;
    public Button ExitRoomBtn;

    //���� �α׸� ǥ���� Text UI �׸� ����
    public Text txtLogMsg;
    //RPC ȣ���� ���� PhotonView
    PhotonView pv;

    //--- ä�� ���� ����
    public InputField InputFdChat;
    public static bool bEnter = false;
    //bool IsWaitCtrl = false;
    //--- ä�� ���� ����

    //--- ������ ���� ������...
    [HideInInspector] public GameState m_OldState = GameState.GS_Ready;
    public static GameState m_GameState = GameState.GS_Ready;

    ExitGames.Client.Photon.Hashtable m_StateProps =
                            new ExitGames.Client.Photon.Hashtable();

    //------ Team Select �κ�
    [Header("--- Team1 UI ---")]
    public GameObject Team1Panel;
    public Button m_Team1ToTeam2;
    public Button m_Team1Ready;
    public GameObject scrollTeam1;

    [Header("--- Team2 UI ---")]
    public GameObject Team2Panel;
    public Button m_Team2ToTeam1;
    public Button m_Team2Ready;
    public GameObject scrollTeam2;

    [Header("--- User Node ---")]
    public GameObject m_UserNodeItem;
    //------ Team Select �κ�

    ExitGames.Client.Photon.Hashtable m_SelTeamProps =
                                new ExitGames.Client.Photon.Hashtable();

    ExitGames.Client.Photon.Hashtable m_PlayerReady =
                                new ExitGames.Client.Photon.Hashtable();

    ExitGames.Client.Photon.Hashtable SitPosInxProps =
                                new ExitGames.Client.Photon.Hashtable();

    [HideInInspector] public static Vector3[] m_Team1Pos = new Vector3[4];
    [HideInInspector] public static Vector3[] m_Team2Pos = new Vector3[4];  
    //--- ������ ���� ������...

    //------- Round ���� ����
    [Header("--- StartTimer UI ---")]
    public Text m_WaitTmText;       //���� ���� �� ī��Ʈ 3, 2, 1, 0
    [HideInInspector] public float m_GoWaitGame = 4.0f;    //���� ���� �� ī��Ʈ Text UI

    int m_RoundCount = 0;       //���� 5����� ����
    [Header("--- WinLossCount ---")]
    public Text m_WinLossCount;     //�¸� ī��Ʈ ǥ�� Text UI
    //------- Round ���� ����

    public Text m_GameEndText;

    void Awake()
    {
        //--- ������ ���� ���� �ʱ�ȭ
        m_Team1Pos[0] = new Vector3(88.4f, 20.0f, 77.9f);
        m_Team1Pos[1] = new Vector3(61.1f, 20.0f, 88.6f);
        m_Team1Pos[2] = new Vector3(34.6f, 20.0f, 98.7f);
        m_Team1Pos[3] = new Vector3(7.7f, 20.0f, 108.9f);

        m_Team2Pos[0] = new Vector3(-19.3f, 20.0f, -134.1f);
        m_Team2Pos[1] = new Vector3(-43.1f, 20.0f, -125.6f);
        m_Team2Pos[2] = new Vector3(-66.7f, 20.0f, -117.3f);
        m_Team2Pos[3] = new Vector3(-91.4f, 20.0f, -108.6f);
 
        m_GameState = GameState.GS_Ready;
        //--- ������ ���� ���� �ʱ�ȭ

        bEnter = false;

        //PhotonView ������Ʈ �Ҵ�
        pv = GetComponent<PhotonView>();

        //��ũ�� �����ϴ� �Լ� ȣ��
        CreateTank();
        //���� Ŭ������ ��Ʈ��ũ �޽��� ������ �ٽ� ����
        PhotonNetwork.IsMessageQueueRunning = true;

        //�뿡 ���� �� ���� ������ ������ ���
        GetConnectPlayerCount();

        //--- CustomProperties �ʱ�ȭ
        InitGStateProps();
        InitSelTeamProps(); //���� ������ �� ���� ������ �ٸ� ����鿡�� ���� �켱 ��������� �����ߴٰ� �˸�
        InitReadyProps();
        //--- CustomProperties �ʱ�ȭ

    }//void Awake()

    // Start is called before the first frame update
    void Start()
    {
        //--- TeamSetting
        //--- ��1 ��ư ó��
        if (m_Team1ToTeam2 != null)
            m_Team1ToTeam2.onClick.AddListener(() =>
            {
                SendSelTeam("black");  //���� �������� �̵��ߴٰ� �߰���(�� ����)
            });

        if (m_Team1Ready != null)
            m_Team1Ready.onClick.AddListener(() =>
            {
                SendReady(1);
            });
        //--- ��1 ��ư ó��

        //--- ��2 ��ư ó��
        if (m_Team2ToTeam1 != null)
            m_Team2ToTeam1.onClick.AddListener(() =>
            {
                SendSelTeam("blue");  //���� ��������� �̵��ߴٰ� �߰���(�� ���� : RPC(AllViaServer))
            });

        if (m_Team2Ready != null)
            m_Team2Ready.onClick.AddListener(() =>
            {
                SendReady(1);
            });
        //--- ��2 ��ư ó��
        //--- TeamSetting

        if (ExitRoomBtn != null)
            ExitRoomBtn.onClick.AddListener(OnClickExitRoom);

        //�α� �޽����� ����� ���ڿ� ����
        string msg = "\n<color=#00ff00>["
                      + PhotonNetwork.LocalPlayer.NickName
                      + "] Connected</color>";

        //RPC �Լ� ȣ��
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, false);
        //RpcTarget.AllBuffered �ɼ��� Ư¡ : �濡�� ���� ������ ���
        //���� ���� ���ۿ� ����� �ִ� RPC�� ���޵��� �ʴ´�.
        //��, �濡 ���� �ִ� �������� �۽� �Ǿ��� ���� �Լ� ����鸸 ���޵ȴ�.
    }

    // Update is called once per frame
    void Update()
    {
        //���� Update()�� ������ �Ǵ� �������� Ȯ���Ѵ�.
        if (IsGamePossible() == false)
            return;

        if(m_GameState == GameState.GS_Ready)
        {
            if(IsDifferentList() == true)
            {
                RefreshPhotonTeam();    //����Ʈ UI ����
            }
        }//if(m_GameState == GameState.GS_Ready)

        //ä�� ���� �ؽ�Ʈ �Է�
        if (Input.GetKeyDown(KeyCode.Return))
        { //����Ű�� ������ ��ǲ �ʵ� Ȱ��ȭ
            bEnter = !bEnter;

            if(bEnter == true)
            {
                InputFdChat.gameObject.SetActive(true);
                InputFdChat.ActivateInputField(); //<-- Ű���� Ŀ�� �Է� ���� ������ ���� ����� ��
            }
            else
            {
                InputFdChat.gameObject.SetActive(false);
                if(string.IsNullOrEmpty(InputFdChat.text.Trim()) == false)
                {
                    BroadcastingChat();
                }
            }
        }//if (Input.GetKeyDown(KeyCode.Return))

        AllReadyObserver();

        if(m_GameState == GameState.GS_Playing)
        {//������ ���۵Ǿ��µ� ���� Team1Panel�̳� m_WaitTmText ������ �ʾҴٸ� Ȯ���� ���� ���� �κ�
            Team1Panel.SetActive(false);
            Team2Panel.SetActive(false);
            m_WaitTmText.gameObject.SetActive(false);
        }//if(m_GameState == GameState.GS_Playing)

        WinLossMgr.Inst.WinLossObserver(this); //������ ���� ���� �� ���� ����

    }////void Update()

    //ä�� ������ �߰��ϴ� �Լ�
    void BroadcastingChat()
    {
        string msg = "\n<color=#ffffff>[" +
                        PhotonNetwork.LocalPlayer.NickName + "] " +
                        InputFdChat.text + "</color>";

        //if(PhotonNetwork.IsMasterClient == true)
        //{
        //    msg = "\n<color=#ffff00>[" +
        //                PhotonNetwork.LocalPlayer.NickName + "] " +
        //                InputFdChat.text + "</color>";
        //}

        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, true);

        InputFdChat.text = "";
    }

    //��ũ�� �����ϴ� �Լ�
    void CreateTank()
    {
        float pos = Random.Range(-100.0f, 100.0f);
        PhotonNetwork.Instantiate("Tank", new Vector3(pos, 20.0f, pos), Quaternion.identity, 0);
    }

    private void OnApplicationFocus(bool focus)
    { //������ â Ȱ��ȭ ��Ȱ��ȭ �� �� ȣ��Ǵ� �Լ�
        isFocus = focus;
        //focus == true �̸� �� â�� ��Ŀ���� �����Դٴ� ��
        //focus == false �̸� �� â�� ��Ŀ���� �Ҿ��ٴ� ��
    }

    //�� ������ ������ ��ȸ�ϴ� �Լ�
    void GetConnectPlayerCount()
    {
        //���� ������ �� ������ �޾ƿ�
        Room currRoom = PhotonNetwork.CurrentRoom; //using Photon.Realtime;

        //���� ���� ������ ���� �ִ� ���� ������ ���� ���ڿ��� ������ �� Text UI �׸� ���
        txtConnect.text = currRoom.PlayerCount.ToString()
                          + "/" 
                          + currRoom.MaxPlayers.ToString();
    }

    //��ũ��ũ �÷��̾ �������� �� ȣ��Ǵ� �Լ�
    public override void OnPlayerEnteredRoom(Player a_Player)
    {
        GetConnectPlayerCount();
    }

    //��Ʈ��ũ �÷��̾ ���� �����ų� ������ �������� �� ȣ��Ǵ� �Լ�
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GetConnectPlayerCount();
    }

    //�� ������ ��ư Ŭ�� �̺�Ʈ�� ������ �Լ�
    public void OnClickExitRoom()
    {
        //�α� �޽����� ����� ���ڿ� ����
        string msg = "\n<color=#ff0000>["
                    + PhotonNetwork.LocalPlayer.NickName
                    + "] Disconnected</color>";
        //RPC �Լ� ȣ��
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, false);
        //������ �Ϸ�� �� ���� ������ ������ ������
        //������ �뿡 ������ ���� ���� �αװ� ǥ�õǴ� ���� Ȯ���� �� �ִ�.
        //���� PhotonTarget.AllBuffered �ɼ�����
        //RPC�� ȣ���߱� ������ ���߿� �����ص�
        //�⺻�� ���� �α� �޽����� ǥ�õȴ�.

        //������ ����� ���� ���� �� ���� CustomProperties�� �ʱ�ȭ �� ��� �Ѵ�.
        if(PhotonNetwork.PlayerList != null && PhotonNetwork.PlayerList.Length <= 1)
        {
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        }

        //���� �������� ��ũ�� ã�Ƽ� �� ��ũ��
        //��� CustomProperties�� �ʱ�ȭ �� �ְ� ������ ���� ����.
        //(�׷��� ������ ������ ��� �� ����� ���� �߻��Ѵ�.)
        if(PhotonNetwork.LocalPlayer != null)
            PhotonNetwork.LocalPlayer.CustomProperties.Clear();
        //���� ó�� CustomProperties�� �ʱ�ȭ �� ���
        //CustomProperties�� �߰�Ǵ� ���� ��� �ʱ�ȭ �� ���̴�.

        //���� ���� ���������� ������ ��� ��Ʈ��ũ ��ü�� ����
        PhotonNetwork.LeaveRoom();
    }

    //�뿡�� ���� ������� �� ȣ��Ǵ� �ݹ� �Լ�
    //PhotonNetwork.LeaveRoom();�� ���� ���� ��
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("scLobby");
    }

    bool IsGamePossible() //������ ������ ��������? üũ�ϴ� �Լ�
    {
        //������ Ÿ�ֿ̹� ���� �������� ���÷��� ���� �������
        //LoadScene()�� ���÷��� �ʰ� ȣ��Ǵ� ���� �ذ��
        if(PhotonNetwork.CurrentRoom == null ||
           PhotonNetwork.LocalPlayer == null)
            return false; //����ȭ ������ �����϶��� ������Ʈ�� ����� �ش�.

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameState") == false ||
            PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Team1Win") == false ||
            PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Team2Win") == false)
            return false;
     
        m_GameState = ReceiveGState();
        WinLossMgr.Inst.m_Team1Win = (int)PhotonNetwork.CurrentRoom.CustomProperties["Team1Win"];
        WinLossMgr.Inst.m_Team2Win = (int)PhotonNetwork.CurrentRoom.CustomProperties["Team2Win"];

        return true;
    }

    public static bool IsPointerOverUIObject() //UGUI�� UI���� ���� ��ŷ�Ǵ��� Ȯ���ϴ� �Լ�
    {
        PointerEventData a_EDCurPos = new PointerEventData(EventSystem.current);

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)

			List<RaycastResult> results = new List<RaycastResult>();
			for (int i = 0; i < Input.touchCount; ++i)
			{
				a_EDCurPos.position = Input.GetTouch(i).position;  
				results.Clear();
				EventSystem.current.RaycastAll(a_EDCurPos, results);
                if (0 < results.Count)
                    return true;
			}

			return false;
#else
        a_EDCurPos.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(a_EDCurPos, results);
        return (0 < results.Count);
#endif
    }//public bool IsPointerOverUIObject() 

    List<string> m_MsgList = new List<string>();
    // info.sender�� �޽����� ���� �÷��̾��� ������ ��� �ֽ��ϴ�.
    //void LogMsg(string msg, PhotonMessageInfo info) //if (info.Sender.IsLocal) // ���� ���� �޽����� ���
    [PunRPC]
    void LogMsg(string msg, bool isChatMsg, PhotonMessageInfo info)
    {
        //���ÿ��� ���� ���� �޽����� ��츸
        //if(info.Sender.IsLocal && msg.Contains("#ffffff") == true) 
        //if(isChatMsg == true && 
        //   senderActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        if (info.Sender.IsLocal && isChatMsg == true)
        {
            msg = msg.Replace("#ffffff", "#ffff00");
        }

        m_MsgList.Add(msg);
        if (20 < m_MsgList.Count)
            m_MsgList.RemoveAt(0);

        txtLogMsg.text = "";
        for(int i = 0; i < m_MsgList.Count; i++)
        {
            txtLogMsg.text += m_MsgList[i];
        }
    }

    private void OnGUI()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        // ������ ���� ���۵��� ���� ����
        // �� "������ ���� : ų�� : �������
        if (PhotonNetwork.CurrentRoom.IsOpen == true)
            return;

        //���� ������ �뿡 ������ ��� ��Ʈ��ũ �÷��̾� ������ ����
        int a_CurHp = 0;
        int curKillCount = 0;
        string PlayerTeam = "blue";
        Player[] players = PhotonNetwork.PlayerList;  //using Photon.Realtime;

        GameObject[] tanks = GameObject.FindGameObjectsWithTag("TANK");
        foreach(Player a_Player in players) 
        {
            curKillCount = 0;
            if (a_Player.CustomProperties.ContainsKey("KillCount") == true)
                curKillCount = (int)a_Player.CustomProperties["KillCount"];

            PlayerTeam = "blue";
            if (a_Player.CustomProperties.ContainsKey("MyTeam") == true)
                PlayerTeam = (string)a_Player.CustomProperties["MyTeam"];

            TankDamage tankDamage = null;
            foreach(GameObject a_Tank in tanks)
            {
                TankDamage a_TankDmg = a_Tank.GetComponent<TankDamage>();
                //��ũ�� PlayerId�� �濡 ������ a_Player.ActorNumber �� �������� �Ǵ�
                if (a_TankDmg == null)
                    continue;

                if(a_TankDmg.PlayerId == a_Player.ActorNumber)
                {
                    tankDamage = a_TankDmg;
                    break;
                }
            }//foreach(GameObject a_Tank in tanks)

            if(tankDamage != null)
            {   //��� ĳ������ �������� ����ȭ
                a_CurHp = tankDamage.currHp;
            }//if(tankDamage != null)

            string a_PrintStr = "";

            string a_sColor = "<color=Blue><size=25>";
            if(PlayerTeam == "black")
                a_sColor = "<color=Black><size=25>";

            a_PrintStr = a_sColor +
                "[" + a_Player.ActorNumber + "] " + a_Player.NickName + " "
                + curKillCount + " kill" + "</size></color>";

            if (a_CurHp <= 0)  //�׾� ���� ��
                a_PrintStr += "<color=Red><size=25>" + " <Die>" + "</size></color>";

            GUILayout.Label(a_PrintStr);

            //if (a_CurHp <= 0)  //�׾� ���� ��
            //{
            //    GUILayout.Label("<color=Blue><size=25>" +
            //        "[" + a_Player.ActorNumber + "] " + a_Player.NickName + " "
            //        + curKillCount + " kill" + "</size></color>"
            //        + "<color=Red><size=25>" + " <Die>" + "</size></color>");
            //}
            //else  //��� ���� ��
            //{
            //    GUILayout.Label("<color=Blue><size=25>" +
            //        "[" + a_Player.ActorNumber + "] " + a_Player.NickName + " "
            //        + curKillCount + " kill" + "</size></color>");
            //}

        }//foreach(Player a_Player in players) 

    }//private void OnGUI()

 #region ---------- ���� ���� ����ȭ ó��

    void InitGStateProps()
    {
        //PhotonNetwork.CurrentRoom <-- ������ �����ϰ� �ִ� �������
        if (PhotonNetwork.CurrentRoom == null)
            return;

        m_StateProps.Clear();
        m_StateProps.Add("GameState", (int)GameState.GS_Ready);
        PhotonNetwork.CurrentRoom.SetCustomProperties(m_StateProps);
    }

    public void SendGState(GameState a_GState)
    {
        if (m_StateProps == null)
        {
            m_StateProps = new ExitGames.Client.Photon.Hashtable();
            m_StateProps.Clear();
        }

        if (m_StateProps.ContainsKey("GameState") == true)
            m_StateProps["GameState"] = (int)a_GState;
        else
            m_StateProps.Add("GameState", (int)a_GState);

        PhotonNetwork.CurrentRoom.SetCustomProperties(m_StateProps);
    }

    GameState ReceiveGState()  //GameState �޾Ƽ� ó���ϴ� �κ�
    {
        GameState a_RmVal = GameState.GS_Ready;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameState") == true)
            a_RmVal = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["GameState"];

        return a_RmVal;
    }

#endregion

#region --------- ������ ����ȭ ó��
    void InitSelTeamProps()
    {   //�ӵ��� ���� ���۸� �̸� ����� ���´ٴ� �ǹ�
        m_SelTeamProps.Clear();
        m_SelTeamProps.Add("MyTeam", "blue");   //�� ����� �⺻�� ��������� �����Ѵ�.
        PhotonNetwork.LocalPlayer.SetCustomProperties(m_SelTeamProps);                                        
        //ĳ���� ���� ����ȭ ��Ű�� ���� ���
    }

    void SendSelTeam(string a_Team)
    {
        if (string.IsNullOrEmpty(a_Team) == true)
            return;

        if(m_SelTeamProps == null)
        {
            m_SelTeamProps = new ExitGames.Client.Photon.Hashtable();
            m_SelTeamProps.Clear();
        }

        if (m_SelTeamProps.ContainsKey("MyTeam") == true)
            m_SelTeamProps["MyTeam"] = a_Team;
        else
            m_SelTeamProps.Add("MyTeam", a_Team);

        PhotonNetwork.LocalPlayer.SetCustomProperties(m_SelTeamProps);
        //ĳ���� ���� ����ȭ ��Ű�� ���� ���
        //(PhotonNetwork.LocalPlayer == pv.IsMine && pv.Owner)

    }//void SendSelTeam(string a_Team)

    public string ReceiveSelTeam(Player a_Player)
    {
        string a_TeamKind = "blue";

        if (a_Player == null)
            return a_TeamKind;

        if (a_Player.CustomProperties.ContainsKey("MyTeam") == true)
            a_TeamKind = (string)a_Player.CustomProperties["MyTeam"];

        return a_TeamKind;  
    }

    bool IsDifferentList() //true�� ��ȭ�� ����ٴ� �� false�� ���ٴ� ��
    {
        GameObject[] a_UserNodeItems = GameObject.FindGameObjectsWithTag("UserNode_Item");

        if (a_UserNodeItems == null)
            return true;

        if(PhotonNetwork.PlayerList.Length != a_UserNodeItems.Length)
            return true;

        foreach(Player a_RefPlayer in PhotonNetwork.PlayerList)
        {
            bool a_FindNode = false;
            UserNodeItem a_UserData = null;
            foreach(GameObject a_Node in a_UserNodeItems)
            {
                a_UserData = a_Node.GetComponent<UserNodeItem>();
                if (a_UserData == null)
                    continue;

                if(a_UserData.m_UniqID == a_RefPlayer.ActorNumber)
                {
                    if (a_UserData.m_TeamKind != ReceiveSelTeam(a_RefPlayer))
                        return true; //�ش� ������ ���� ���� �Ǿ��ٸ�...

                    if(a_UserData.m_IamReady != ReceiveReady(a_RefPlayer))
                        return true; //�ش� Ready ���°� ���� �Ǿ��ٸ�...

                    a_FindNode = true;
                    break;
                }
            }//foreach(GameObject a_Node  in a_UserNodeItems)

            if(a_FindNode == false)
                return true; //�ش� ������ ����Ʈ�� �������� ������... ���� �ʿ�

        }//foreach(Player a_RefPlayer in PhotonNetwork.PlayerList)

        return false;   //��ġ�Ѵٴ� ��

    }//bool IsDifferentList() //true�� ��ȭ�� ����ٴ� �� false�� ���ٴ� ��

    void RefreshPhotonTeam() //�� ���� ����Ʈ�� UI �� ������ �ִ� �Լ�
    {
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("UserNode_Item"))
        {
            Destroy(obj);
        }

        string a_TeamKind = "blue";
        GameObject a_UserNode = null;
        foreach(Player a_RefPlayer in PhotonNetwork.PlayerList)
        {
            a_TeamKind = ReceiveSelTeam(a_RefPlayer);
            a_UserNode = Instantiate(m_UserNodeItem);

            //���� ����?�� ���� ��ũ�� �並 �б� �� �ش�.
            if (a_TeamKind == "blue")
                a_UserNode.transform.SetParent(scrollTeam1.transform, false);
            else if (a_TeamKind == "black")
                a_UserNode.transform.SetParent(scrollTeam2.transform, false);

            //������ UserNodeItem�� ǥ���ϱ� ���� �ؽ�Ʈ ���� ����
            UserNodeItem a_UsData = a_UserNode.GetComponent<UserNodeItem>();
            if(a_UsData != null)
            {
                a_UsData.m_UniqID = a_RefPlayer.ActorNumber;
                a_UsData.m_TeamKind = a_TeamKind;
                a_UsData.m_IamReady = ReceiveReady(a_RefPlayer);
                bool isMine = (a_UsData.m_UniqID == PhotonNetwork.LocalPlayer.ActorNumber);
                a_UsData.DispPlayerData(a_RefPlayer.NickName, isMine);
            }//if(a_UsData != null)

        }//foreach(Player a_RefPlayer in PhotonNetwork.PlayerList)

        //�̸�ǥ ���� �ٲٱ�
        DisplayUserId a_DpUserId = null;
        GameObject[] a_Tanks = GameObject.FindGameObjectsWithTag("TANK");
        foreach(GameObject tank in a_Tanks)
        {
            a_DpUserId = tank.GetComponent<DisplayUserId>();
            if (a_DpUserId == null)
                continue;

            a_DpUserId.ChangeTankNameColor(this);

        }//foreach(GameObject tank in a_Tanks)
         //�̸�ǥ ���� �ٲٱ�

        //--- ���� Ready ���¿� ���� UI ������ �ֱ�
        if (ReceiveReady(PhotonNetwork.LocalPlayer) == true)
        {  //���� Ready ���¶��...
            m_Team1Ready.gameObject.SetActive(false);
            m_Team2Ready.gameObject.SetActive(false);

            m_Team1ToTeam2.gameObject.SetActive(false);
            m_Team2ToTeam1.gameObject.SetActive(false);
        }
        else  //���� ���� Ready ���°� �ƴ϶��...
        {
            a_TeamKind = ReceiveSelTeam(PhotonNetwork.LocalPlayer);
            if (a_TeamKind == "blue")
            {
                m_Team1Ready.gameObject.SetActive(true);
                m_Team2Ready.gameObject.SetActive(false);
                m_Team1ToTeam2.gameObject.SetActive(true);
                m_Team2ToTeam1.gameObject.SetActive(false);
            }
            else if (a_TeamKind == "black")
            {
                m_Team1Ready.gameObject.SetActive(false);
                m_Team2Ready.gameObject.SetActive(true);
                m_Team1ToTeam2.gameObject.SetActive(false);
                m_Team2ToTeam1.gameObject.SetActive(true);
            }
        }//else  //���� ���� Ready ���°� �ƴ϶��...
        //--- ���� Ready ���¿� ���� UI ������ �ֱ�

    }//void RefreshPhotonTeam() //�� ���� ����Ʈ�� UI �� ������ �ִ� �Լ�
#endregion --------- ������ ����ȭ ó��

#region --------- Ready ���� ����ȭ ó��

    void InitReadyProps()
    { //�ӵ��� ���� ���۸� �̸� ����� ���´ٴ� �ǹ�
        m_PlayerReady.Clear();
        m_PlayerReady.Add("IamReady", 0);   //�⺻������ ���� �غ��� ���·� �����Ѵ�.
        PhotonNetwork.LocalPlayer.SetCustomProperties(m_PlayerReady);
        //ĳ���ͺ� ����ȭ
    }

    void SendReady(int a_Ready = 1)
    {
        if(m_PlayerReady == null)
        {
            m_PlayerReady = new ExitGames.Client.Photon.Hashtable();
            m_PlayerReady.Clear();
        }

        if (m_PlayerReady.ContainsKey("IamReady") == true)
            m_PlayerReady["IamReady"] = a_Ready;
        else
            m_PlayerReady.Add("IamReady", a_Ready);

        PhotonNetwork.LocalPlayer.SetCustomProperties(m_PlayerReady);
        //ĳ���ͺ� ����ȭ
    }

    bool ReceiveReady(Player a_Player)
    {
        if (a_Player == null)
            return false;

        if (a_Player.CustomProperties.ContainsKey("IamReady") == false)
            return false;

        if ((int)a_Player.CustomProperties["IamReady"] == 1)
            return true;

        return false;
    }

    #endregion ---------  Ready ���� ����ȭ ó��

#region ------------ Observer Method ����
    
    //������ ���� ��� Ready ��ư �������� �����ϰ� ������ �����ϰ� ó���ϴ� �Լ�
    void AllReadyObserver()
    {
        if (m_GameState != GameState.GS_Ready) //GS_Ready ���׿����� Ȯ���Ѵ�.
            return;

        int a_OldGoWait = (int)m_GoWaitGame;

        bool a_AllReady = true;
        foreach(Player a_RefPlayer in PhotonNetwork.PlayerList)
        {
            if(ReceiveReady(a_RefPlayer) == false)
            {
                a_AllReady = false;
                break;
            }
        }//foreach(Player a_RefPlayer in PhotonNetwork.PlayerList)

        if(a_AllReady == true)  //��ΰ� �غ� ��ư�� ����� ��ٸ��� �ִٴ� ��
        {
            //���� �߻����׵� ����ȭ ��Ű���� �ϸ�...
            if(m_RoundCount == 0 && PhotonNetwork.CurrentRoom.IsOpen == true)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                //������ ���۵Ǹ� �ٸ� ���� ������ ���ϵ��� ���� �κ�
                //PhotonNetwork.CurrentRoom.IsVisible = false;
                //�κ񿡼� �� ��Ͽ����� ������ �ʰ� �ϱ�...
            }

            //--- �� �÷��̾� PC ���� 3, 2, 1, 0 Ÿ�̸� UI ǥ�ø� ���� �ڵ�
            if(0.0f < m_GoWaitGame)  //Ÿ�̸� ī��Ʈ ó��
            {
                m_GoWaitGame -= Time.deltaTime;
                if(m_WaitTmText != null)
                {
                    m_WaitTmText.gameObject.SetActive(true);
                    m_WaitTmText.text = ((int)m_GoWaitGame).ToString();
                }

                //������ Ŭ���̾�Ʈ�� �� ������ �ڸ���ġ�� �� �� ���̴�.
                //�� 3���� ������. MasterClient�� ���� ��츦 ����ؼ�...
                if (PhotonNetwork.IsMasterClient == true)
                if (0.0f < m_GoWaitGame && a_OldGoWait != (int)m_GoWaitGame)
                {
                    //�ڸ� ����
                    SitPosInxMasterCtrl();
                } //1�ʿ� �ѹ��� 3�� �ڸ� ����
     
                if(m_GoWaitGame <= 0.0f) //Ÿ�� �ƿ� �Ǿ��� �� �ѹ��� �߻��� ����
                {//��¥ ���� ���� �غ�
                    m_RoundCount++;

                    Team1Panel.SetActive(false);
                    Team2Panel.SetActive(false);
                    m_WaitTmText.gameObject.SetActive(false);

                    WinLossMgr.Inst.m_CheckWinTime = 2.0f;
                    m_GoWaitGame = 0.0f;

                }//if(m_GoWaitGame <= 0.0f) //Ÿ�� �ƿ� �Ǿ��� �� �ѹ��� �߻��� ����

            }//if(0.0f < m_GoWaitGame)  //Ÿ�̸� ī��Ʈ ó��
            //---- �� �÷��̾� PC ���� Ÿ�̸� UI ǥ�ø� ���� �ڵ�

            //������ ���� �Ǿ���� �ϴµ� ���� ���� ���� �ʾҴٸ�...
            if(PhotonNetwork.IsMasterClient == true) //������ Ŭ���̾�Ʈ�� üũ�ϰ� ������.
            if(m_GoWaitGame <= 0.0f)
            {
                SendGState(GameState.GS_Playing);
            }

        }//if(a_AllReady == true)  //��ΰ� �غ� ��ư�� ����� ��ٸ��� �ִٴ� ��
    }//void AllReadyObserver()

    void SitPosInxMasterCtrl()
    {
        int a_Tm1Count = 0;
        int a_Tm2Count = 0;
        string a_TeamKind = "blue";
        foreach(Player _player in PhotonNetwork.PlayerList)
        {
            if (_player.CustomProperties.ContainsKey("MyTeam") == true)
                a_TeamKind = (string)_player.CustomProperties["MyTeam"];

            if(a_TeamKind == "blue")
            {
                SitPosInxProps.Clear();
                SitPosInxProps.Add("SitPosInx", a_Tm1Count);
                _player.SetCustomProperties(SitPosInxProps);
                a_Tm1Count++;
            }
            else if(a_TeamKind == "black")
            {
                SitPosInxProps.Clear();
                SitPosInxProps.Add("SitPosInx", a_Tm2Count);
                _player.SetCustomProperties(SitPosInxProps);
                a_Tm2Count++;
            }
        }//foreach(Player _player in PhotonNetwork.PlayerList)
    }//void SitPosInxMasterCtrl()

    #endregion

}
