using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public enum GameState { G_Ready = 0, G_Play, G_End}

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
    public Button  m_Switch1_Btn;
    public Button  m_Ready1_Btn;

    [Header("Team2 Select")]
    public GameObject Team2_Panel;
    public GameObject m_Team2_Scroll;
    public Button  m_Switch2_Btn;
    public Button  m_Ready2_Btn;

    [Header("User Node")]
    public GameObject m_UserNdItem;



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
    }

    // Start is called before the first frame update
    void Start()
    {
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



}
