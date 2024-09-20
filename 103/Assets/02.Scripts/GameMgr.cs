using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public enum GameState { G_Ready = 0, G_Play, G_End }

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

    //�������� ���� ����
    GameState m_GState = GameState.G_Ready;

    public static GameState GState = GameState.G_Ready;

    ExitGames.Client.Photon.Hashtable m_StateProp = new ExitGames.Client.Photon.Hashtable();

    //�� ����
    [Header("Team1 Select")]
    public GameObject Team1_Panel;
    public GameObject m_Team1_Scroll;
    public Button m_Switch1_Btn;
    public Button m_Ready1_Btn;

    [Header("Team2 Select")]
    public GameObject Team2_Panel;
    public GameObject m_Team2_Scroll;
    public Button m_Switch2_Btn;
    public Button m_Ready2_Btn;

    [Header("User Node")]
    public GameObject m_UserNdItem;


    //# Round 
    [Header("StartTimer")]
    public Text m_WaitTmTxt;
    [HideInInspector] public float m_StWait = 5.0f;

    int m_RoundCnt = 0;
    double m_ChckWinTime = 2.0f;


    //# ������ �߰�
    ExitGames.Client.Photon.Hashtable m_SelProps = new ExitGames.Client.Photon.Hashtable();

    //# ������� �߰�
    ExitGames.Client.Photon.Hashtable m_ReadyProps = new ExitGames.Client.Photon.Hashtable();

    void Awake()
    {
        //#������ �ʱ�ȭ
        m_GState = GameState.G_Ready;

        bEnter = false;

        //PhotonView ������Ʈ �Ҵ�
        pv = GetComponent<PhotonView>();

        //��ũ�� �����ϴ� �Լ� ȣ��
        CreateTank();
        //���� Ŭ������ ��Ʈ��ũ �޽��� ������ �ٽ� ����
        PhotonNetwork.IsMessageQueueRunning = true;

        //�뿡 ���� �� ���� ������ ������ ���
        GetConnectPlayerCount();

        //CustomProperties �ʱ�ȭ
        InitSelTm_Props();

        //Ready ���� �ʱ�ȭ
        InitReady();

        //���� ���� �ʱ�ȭ
        InitGState();

    }

    // Start is called before the first frame update
    void Start()
    {
        //## �� ���� ��ư �̺�Ʈ ����
        if (m_Switch1_Btn != null)
            m_Switch1_Btn.onClick.AddListener(() => { SendSelTm("Red"); });

        if (m_Switch2_Btn != null)
            m_Switch2_Btn.onClick.AddListener(() => { SendSelTm("Blue"); });

        //## Ready ��ư �̺�Ʈ ����
        if (m_Ready1_Btn != null)
            m_Ready1_Btn.onClick.AddListener(() => { SendReady(1); });

        if (m_Ready2_Btn != null)
            m_Ready2_Btn.onClick.AddListener(() => { SendReady(1); });


        if (ExitRoomBtn != null)
            ExitRoomBtn.onClick.AddListener(OnClickExitRoom);

        //�α� �޽����� ����� ���ڿ� ����
        string msg = "\n<color=#00ff00>["
                      + PhotonNetwork.LocalPlayer.NickName
                      + "] Connected</color>";

        //RPC �Լ� ȣ��
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, false);


    }

    // Update is called once per frame
    void Update()
    {
        //���� Update()�� ������ �Ǵ� �������� Ȯ���Ѵ�.
        if (IsGamePossible() == false)
            return;

        if (m_GState == GameState.G_Ready)
        {
            if (IsDiffList() == true)
            {
                RefreshPhotonTm();
            }
        }


        //ä�� ���� �ؽ�Ʈ �Է�
        if (Input.GetKeyDown(KeyCode.Return))
        { //����Ű�� ������ ��ǲ �ʵ� Ȱ��ȭ
            bEnter = !bEnter;

            if (bEnter == true)
            {
                InputFdChat.gameObject.SetActive(true);
                InputFdChat.ActivateInputField(); //<-- Ű���� Ŀ�� �Է� ���� ������ ���� ����� ��
            }
            else
            {
                InputFdChat.gameObject.SetActive(false);
                if (string.IsNullOrEmpty(InputFdChat.text.Trim()) == false)
                {
                    BroadcastingChat();
                }
            }
        }//if (Input.GetKeyDown(KeyCode.Return))

        A_ReadyOb();

        if (m_GState == GameState.G_Play)
        {
            Team1_Panel.gameObject.SetActive(false);
            Team2_Panel.gameObject.SetActive(false);
            m_WaitTmTxt.gameObject.SetActive(false);
        }

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
        if (PhotonNetwork.PlayerList != null && PhotonNetwork.PlayerList.Length <= 1)
        {
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        }

        //���� �������� ��ũ�� ã�Ƽ� �� ��ũ��
        //��� CustomProperties�� �ʱ�ȭ �� �ְ� ������ ���� ����.
        //(�׷��� ������ ������ ��� �� ����� ���� �߻��Ѵ�.)
        if (PhotonNetwork.LocalPlayer != null)
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

    public static bool IsGamePossible() //������ ������ ��������? üũ�ϴ� �Լ�
    {
        //������ Ÿ�ֿ̹� ���� �������� ���÷��� ���� �������
        //LoadScene()�� ���÷��� �ʰ� ȣ��Ǵ� ���� �ذ��
        if (PhotonNetwork.CurrentRoom == null ||
           PhotonNetwork.LocalPlayer == null)
            return false; //����ȭ ������ �����϶��� ������Ʈ�� ����� �ش�.

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

    [PunRPC]
    void LogMsg(string msg, bool IsChatMsg, PhotonMessageInfo info)
    {
        ////���ÿ��� ���� ���� �޽����� ��츸
        //if(info.Sender.IsLocal && msg.Contains("#ffffff") == true) 
        //{
        //    Debug.Log(info.Sender.IsLocal);
        //    msg = msg.Replace("#ffffff", "#ffff00");
        //}

        if (info.Sender.IsLocal && IsChatMsg == true)
        {
            msg = msg.Replace("#ffffff", "#ffff00");
        }


        m_MsgList.Add(msg);
        if (20 < m_MsgList.Count)
            m_MsgList.RemoveAt(0);

        txtLogMsg.text = "";
        for (int i = 0; i < m_MsgList.Count; i++)
        {
            txtLogMsg.text += m_MsgList[i];
        }
    }

    void OnGUI()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        if (PhotonNetwork.CurrentRoom.IsOpen == true) return;

        //������ ���۵��������� ���� ���� ų�� ������� ǥ��

        //���� ������ �濡 ������ ��� �÷��̾� ���� ����
        int a_CurHP = 0;
        int a_CurKillCnt = 0;

        Player[] a_Players = PhotonNetwork.PlayerList;

        GameObject[] a_Tanks = GameObject.FindGameObjectsWithTag("Tank");

        foreach (Player a_Player in a_Players)
        {
            a_CurKillCnt = 0;

            if (a_Player.CustomProperties.ContainsKey("KillCount") == true)
            {
                a_CurKillCnt = (int)a_Player.CustomProperties["KillCount"];
            }

            TankDamage a_TankDamage = null;

            foreach (GameObject a_Tank in a_Tanks)
            {
                TankDamage a_TankDmg = a_Tank.GetComponent<TankDamage>();


                if (a_TankDmg == null) continue;

                if (a_TankDmg.PlayerId == a_Player.ActorNumber)
                {
                    a_TankDamage = a_TankDmg;
                    break;
                }


            }


            if (a_TankDamage != null)
            {
                a_CurHP = a_TankDamage.currHp;
            }

            if (a_CurHP <= 0)
            {
                GUILayout.Label("<color=Blue><size=25>" + "[" + a_Player.ActorNumber + "] "
                    + a_Player.NickName + " : " + a_CurKillCnt + "kill" + "</size></color>"
                    + "<color=Red><size=25>" + "Dead!" + "</size></color>");
            }
            else
            {
                GUILayout.Label("<color=Blue><size=25>" + "["
                    + a_Player.ActorNumber + "] "
                                       + a_Player.NickName + " : "
                                       + a_CurKillCnt + "kill" + "</size></color>");
            }

        }


    }

    #region ���� ���� ����ȭ
    void InitGState()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        m_StateProp.Clear();

        m_StateProp.Add("GameState", (int)GameState.G_Ready);

        PhotonNetwork.CurrentRoom.SetCustomProperties(m_StateProp);

    }

    void SendGState(GameState a_GState)
    {
        if (m_StateProp == null)
        {
            m_StateProp = new ExitGames.Client.Photon.Hashtable();
            m_StateProp.Clear();
        }

        if (m_StateProp.ContainsKey("GameState") == true)
            m_StateProp["GameState"] = (int)a_GState;
        else
            m_StateProp.Add("GameState", (int)a_GState);

        PhotonNetwork.CurrentRoom.SetCustomProperties(m_StateProp);
    }

    GameState ReciveGState()
    {
        GameState a_RVal = GameState.G_Ready;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameState") == true)
            a_RVal = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["GameState"];

        return a_RVal;
    }
    #endregion

    #region �� ���� ����ȭ
    void InitSelTm_Props()
    {
        //�� ������ ���� CustomProperties �ʱ�ȭ
        m_SelProps.Clear();
        m_SelProps.Add("MyTeam", "Blue");
        PhotonNetwork.LocalPlayer.SetCustomProperties(m_SelProps);
    }

    void SendSelTm(string a_Team)
    {
        if (string.IsNullOrEmpty(a_Team)) return;

        if (m_SelProps == null)
        {
            m_SelProps = new ExitGames.Client.Photon.Hashtable();
            m_SelProps.Clear();
        }

        if (m_SelProps.ContainsKey("MyTeam") == true)
            m_SelProps["MyTeam"] = a_Team;
        else
            m_SelProps.Add("MyTeam", a_Team);

        PhotonNetwork.LocalPlayer.SetCustomProperties(m_SelProps);//IsMine�� �ش�Ǵ� �÷��̾ �����ϴ� �ٸ� ���(= pc.owner)

    }
    public string ReciveSelTm(Player a_Player)
    {
        string a_TeamKnd = "Blue";

        if (a_Player == null) return a_TeamKnd;

        if (a_Player.CustomProperties.ContainsKey("MyTeam") == true)
            a_TeamKnd = (string)a_Player.CustomProperties["MyTeam"];

        return a_TeamKnd;


    }

    bool IsDiffList()
    {
        GameObject[] a_UNdItems = GameObject.FindGameObjectsWithTag("UserNd_Item");

        if (a_UNdItems == null)
            return true;

        if (PhotonNetwork.PlayerList.Length != a_UNdItems.Length)
            return true;

        foreach (Player a_Player in PhotonNetwork.PlayerList)
        {
            bool IsFind = false;
            UserNdItem a_UserData = null;

            foreach (GameObject a_Node in a_UNdItems)
            {
                a_UserData = a_Node.GetComponent<UserNdItem>();

                if (a_UserData == null) continue;

                if (a_UserData.m_Uniq_ID == a_Player.ActorNumber)
                {
                    if (a_UserData.m_TeamKnd != ReciveSelTm(a_Player))
                        return true;

                    if (a_UserData.IsReady != ReciveReady(a_Player))
                        return true;

                    IsFind = true;
                    break;

                }

            }

            if (IsFind == false)
                return true;
        }


        return false; //��ġ(��ȭ�� ����)
    }

    void RefreshPhotonTm()
    {
        foreach (GameObject a_Obj in GameObject.FindGameObjectsWithTag("UserNd_Item"))
        {
            Destroy(a_Obj);
        }



        string a_TeamKnd = "Blue";

        GameObject a_UserNd = null;

        foreach (Player a_Player in PhotonNetwork.PlayerList)
        {
            a_TeamKnd = ReciveSelTm(a_Player);
            a_UserNd = Instantiate(m_UserNdItem);

            //�� �Ҽӿ� ���� �б� ó��
            if (a_TeamKnd == "Blue")
                a_UserNd.transform.SetParent(m_Team1_Scroll.transform, false);

            else if (a_TeamKnd == "Red")
                a_UserNd.transform.SetParent(m_Team2_Scroll.transform, false);

            //���� ������� �����ۿ� �ؽ�Ʈ ���� ����
            UserNdItem a_UserNdData = a_UserNd.GetComponent<UserNdItem>();

            if (a_UserNdData != null)
            {
                a_UserNdData.m_Uniq_ID = a_Player.ActorNumber;
                a_UserNdData.m_TeamKnd = a_TeamKnd;
                a_UserNdData.IsReady = ReciveReady(a_Player);

                bool IsMine = (a_UserNdData.m_Uniq_ID == PhotonNetwork.LocalPlayer.ActorNumber);

                a_UserNdData.DispPlayerData(a_Player.NickName, IsMine);
            }

        }

        //�̸�ǥ �� ����
        DisplayUserId a_DispId = null;
        GameObject[] a_Tanks = GameObject.FindGameObjectsWithTag("Tank");
        foreach(GameObject tank in a_Tanks)
        {
            a_DispId = tank.GetComponent<DisplayUserId>();
            if (a_DispId == null) continue;

            a_DispId.ChangeTN_Color(this);
        }



        //## ���� Ready ���� ǥ��
        a_TeamKnd = ReciveSelTm(PhotonNetwork.LocalPlayer);

        if (ReciveReady(PhotonNetwork.LocalPlayer) == true)
        {
            m_Switch1_Btn.gameObject.SetActive(false);
            m_Switch2_Btn.gameObject.SetActive(false);

            m_Switch1_Btn.gameObject.SetActive(false);
            m_Switch2_Btn.gameObject.SetActive(false);

        }
        else
        {
            if (a_TeamKnd == "Blue")
            {
                m_Ready1_Btn.gameObject.SetActive(true);
                m_Ready2_Btn.gameObject.SetActive(false);
                m_Switch1_Btn.gameObject.SetActive(true);
                m_Switch2_Btn.gameObject.SetActive(false);
            }
            else if (a_TeamKnd == "Red")
            {
                m_Ready2_Btn.gameObject.SetActive(true);
                m_Ready1_Btn.gameObject.SetActive(false);
                m_Switch2_Btn.gameObject.SetActive(true);
                m_Switch1_Btn.gameObject.SetActive(false);
            }
        }
    }

    #endregion

    #region Ready ���� ����ȭ
    void InitReady()
    {
        m_ReadyProps.Clear();
        m_ReadyProps.Add("Ready", 0);

        PhotonNetwork.LocalPlayer.SetCustomProperties(m_ReadyProps);
    }

    void SendReady(int a_Ready = 1)
    {
        if (m_ReadyProps == null)
        {
            m_ReadyProps = new ExitGames.Client.Photon.Hashtable();
            m_ReadyProps.Clear();
        }

        if (m_ReadyProps.ContainsKey("Ready") == true)
            m_ReadyProps["Ready"] = a_Ready;
        else
            m_ReadyProps.Add("Ready", a_Ready);

        PhotonNetwork.LocalPlayer.SetCustomProperties(m_ReadyProps);

    }
    bool ReciveReady(Player a_Player)
    {

        if (a_Player == null) return false;

        if (a_Player.CustomProperties.ContainsKey("Ready") == false)
            return false;
        if ((int)a_Player.CustomProperties["Ready"] == 1)
            return true;

        return false;
    }

    #endregion

    #region Observer Methods
    void A_ReadyOb()
    {
        if (m_GState != GameState.G_Ready) return;

        int a_WaitTm = (int)m_StWait;

        bool IsAReady = true;

        foreach (Player a_Player in PhotonNetwork.PlayerList)
        {
            if (ReciveReady(a_Player) == false)
            {
                IsAReady = false;
                break;
            }
        }

        if (IsAReady == true)
        {
            if (m_RoundCnt == 0 && PhotonNetwork.IsMasterClient == true)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;

                //���� ����
                //PhotonNetwork.CurrentRoom.IsVisible = false;
            }

            //�� �÷��̾� PC���� Ready Ÿ�̸� UIǥ��
            if (0 < a_WaitTm)
            {
                m_StWait -= Time.deltaTime;
                if (m_WaitTmTxt != null)
                {
                    m_WaitTmTxt.gameObject.SetActive(true);
                    m_WaitTmTxt.text = ((int)m_StWait).ToString();
                }

                //������ Ŭ���̾�Ʈ�� �� ���� �ڸ� ��ġ(3�� ������ *�����Ͱ� �����°�� ����)
                if (PhotonNetwork.IsMasterClient == true)
                    if (0 < a_WaitTm && a_WaitTm != (int)m_StWait)

                    {

                    }

                //time out
                if (m_StWait < 0)
                {
                    m_RoundCnt++;

                    Team1_Panel.SetActive(false);
                    Team2_Panel.SetActive(false);

                    m_WaitTmTxt.gameObject.SetActive(false);

                    m_ChckWinTime = 2.0f;
                    m_StWait = 0.0f;

                }

            }

            if (PhotonNetwork.IsMasterClient == true)
                if (m_StWait <= 0)
                {
                    //SendGState(GameState.G_Play);
                }

        }

    }

    #endregion

}
