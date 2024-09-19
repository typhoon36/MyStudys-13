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
    //이 클라이언트 윈도우가 마우스 포커스를 가지고 있는지? 확인하는 변수

    //접속된 플에이어 수를 표시할 Text UI 항목 변수
    public Text txtConnect;
    public Button ExitRoomBtn;

    //접속 로그를 표시할 Text UI 항목 변수
    public Text txtLogMsg;
    //RPC 호출을 위한 PhotonView
    PhotonView pv;

    //--- 채팅 관련 변수
    public InputField InputFdChat;
    public static bool bEnter = false;
    //bool IsWaitCtrl = false;
    //--- 채팅 관련 변수

    //팀대전을 위한 변수
    GameState m_GState = GameState.G_Ready;

    public static GameState GState = GameState.G_Ready;

    ExitGames.Client.Photon.Hashtable m_StateProp = new ExitGames.Client.Photon.Hashtable();

    //팀 선택
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
        //#팀대전 초기화
        m_GState = GameState.G_Ready;

        bEnter = false;

        //PhotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();

        //탱크를 생성하는 함수 호출
        CreateTank();
        //포톤 클라우드의 네트워크 메시지 수신을 다시 연결
        PhotonNetwork.IsMessageQueueRunning = true;

        //룸에 입장 후 기존 접속자 정보를 출력
        GetConnectPlayerCount();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (ExitRoomBtn != null)
            ExitRoomBtn.onClick.AddListener(OnClickExitRoom);

        //로그 메시지에 출력할 문자열 생성
        string msg = "\n<color=#00ff00>["
                      + PhotonNetwork.LocalPlayer.NickName
                      + "] Connected</color>";

        //RPC 함수 호출
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, false);


    }

    // Update is called once per frame
    void Update()
    {
        //게임 Update()를 돌려도 되는 상태인지 확인한다.
        if (IsGamePossible() == false)
            return;

        //채팅 구현 텍스트 입력
        if (Input.GetKeyDown(KeyCode.Return))
        { //엔터키를 누르면 인풋 필드 활성화
            bEnter = !bEnter;

            if (bEnter == true)
            {
                InputFdChat.gameObject.SetActive(true);
                InputFdChat.ActivateInputField(); //<-- 키보드 커서 입력 상자 쪽으로 가게 만들어 줌
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

    //채팅 내용을 중계하는 함수
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

    //탱크를 생성하는 함수
    void CreateTank()
    {
        float pos = Random.Range(-100.0f, 100.0f);
        PhotonNetwork.Instantiate("Tank", new Vector3(pos, 20.0f, pos), Quaternion.identity, 0);
    }

    private void OnApplicationFocus(bool focus)
    { //윈도우 창 활성화 비활성화 될 때 호출되는 함수
        isFocus = focus;
        //focus == true 이면 이 창에 포커스를 가져왔다는 뜻
        //focus == false 이면 이 창에 포커스를 잃었다는 뜻
    }

    //룸 접속자 정보를 조회하는 함수
    void GetConnectPlayerCount()
    {
        //현재 입장한 룸 정보를 받아옴
        Room currRoom = PhotonNetwork.CurrentRoom; //using Photon.Realtime;

        //현재 룸의 접속자 수와 최대 접속 가능한 수를 문자열로 구성한 후 Text UI 항목에 출력
        txtConnect.text = currRoom.PlayerCount.ToString()
                          + "/"
                          + currRoom.MaxPlayers.ToString();
    }

    //네크워크 플레이어가 접속했을 때 호출되는 함수
    public override void OnPlayerEnteredRoom(Player a_Player)
    {
        GetConnectPlayerCount();
    }

    //네트워크 플레이어가 룸을 나가거나 접속이 끊어졌을 때 호출되는 함수
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GetConnectPlayerCount();
    }

    //룸 나가기 버튼 클릭 이벤트에 연결할 함수
    public void OnClickExitRoom()
    {
        //로그 메시지에 출력할 문자열 생성
        string msg = "\n<color=#ff0000>["
                    + PhotonNetwork.LocalPlayer.NickName
                    + "] Disconnected</color>";
        //RPC 함수 호출
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, false);
        //설정이 완료된 후 빌드 파일을 여러개 실행해
        //동일한 룸에 입장해 보면 접속 로그가 표시되는 것을 확인할 수 있다.
        //또한 PhotonTarget.AllBuffered 옵션으로
        //RPC를 호출했기 때문에 나중에 입장해도
        //기본의 접속 로그 메시지가 표시된다.

        //마지막 사람이 방을 떠날 때 룸의 CustomProperties를 초기화 해 줘야 한다.
        if (PhotonNetwork.PlayerList != null && PhotonNetwork.PlayerList.Length <= 1)
        {
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        }

        //지금 나가려는 탱크를 찾아서 그 탱크의
        //모든 CustomProperties를 초기화 해 주고 나가는 것이 좋다.
        //(그렇지 않으면 나갔다 즉시 방 입장시 오류 발생한다.)
        if (PhotonNetwork.LocalPlayer != null)
            PhotonNetwork.LocalPlayer.CustomProperties.Clear();
        //위에 처럼 CustomProperties를 초기화 해 줘야
        //CustomProperties로 중계되던 것이 모두 초기화 될 것이다.

        //현재 룸을 빠져나가며 생성한 모든 네트워크 객체를 삭제
        PhotonNetwork.LeaveRoom();
    }

    //룸에서 접속 종료됐을 때 호출되는 콜백 함수
    //PhotonNetwork.LeaveRoom();이 성공 했을 때
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("scLobby");
    }

    public static bool IsGamePossible() //게임이 가능한 상태인지? 체크하는 함수
    {
        //나가는 타이밍에 포톤 정보들이 한플레임 먼저 사라지고
        //LoadScene()이 한플레임 늦게 호출되는 문제 해결법
        if (PhotonNetwork.CurrentRoom == null ||
           PhotonNetwork.LocalPlayer == null)
            return false; //동기화 가능한 상태일때만 업데이트를 계산해 준다.

        return true;
    }

    public static bool IsPointerOverUIObject() //UGUI의 UI들이 먼저 피킹되는지 확인하는 함수
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
        ////로컬에서 내가 보낸 메시지인 경우만
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

        //게임이 시작되지않으면 유저 별명 킬수 사망상태 표시

        //현재 입장한 방에 접속한 모든 플레이어 정보 저장
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
