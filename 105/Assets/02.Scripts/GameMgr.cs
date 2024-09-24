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

    //--- 팀대전 관련 변수들...
    [HideInInspector] public GameState m_OldState = GameState.GS_Ready;
    public static GameState m_GameState = GameState.GS_Ready;

    ExitGames.Client.Photon.Hashtable m_StateProps =
                            new ExitGames.Client.Photon.Hashtable();

    //------ Team Select 부분
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
    //------ Team Select 부분

    ExitGames.Client.Photon.Hashtable m_SelTeamProps =
                                new ExitGames.Client.Photon.Hashtable();

    ExitGames.Client.Photon.Hashtable m_PlayerReady =
                                new ExitGames.Client.Photon.Hashtable();

    ExitGames.Client.Photon.Hashtable SitPosInxProps =
                                new ExitGames.Client.Photon.Hashtable();

    [HideInInspector] public static Vector3[] m_Team1Pos = new Vector3[4];
    [HideInInspector] public static Vector3[] m_Team2Pos = new Vector3[4];  
    //--- 팀대전 관련 변수들...

    //------- Round 관련 변수
    [Header("--- StartTimer UI ---")]
    public Text m_WaitTmText;       //게임 시작 후 카운트 3, 2, 1, 0
    [HideInInspector] public float m_GoWaitGame = 4.0f;    //게임 시작 후 카운트 Text UI

    int m_RoundCount = 0;       //라운드 5라운드로 진행
    [Header("--- WinLossCount ---")]
    public Text m_WinLossCount;     //승리 카운트 표시 Text UI
    //------- Round 관련 변수

    public Text m_GameEndText;

    void Awake()
    {
        //--- 팀대전 관련 변수 초기화
        m_Team1Pos[0] = new Vector3(88.4f, 20.0f, 77.9f);
        m_Team1Pos[1] = new Vector3(61.1f, 20.0f, 88.6f);
        m_Team1Pos[2] = new Vector3(34.6f, 20.0f, 98.7f);
        m_Team1Pos[3] = new Vector3(7.7f, 20.0f, 108.9f);

        m_Team2Pos[0] = new Vector3(-19.3f, 20.0f, -134.1f);
        m_Team2Pos[1] = new Vector3(-43.1f, 20.0f, -125.6f);
        m_Team2Pos[2] = new Vector3(-66.7f, 20.0f, -117.3f);
        m_Team2Pos[3] = new Vector3(-91.4f, 20.0f, -108.6f);
 
        m_GameState = GameState.GS_Ready;
        //--- 팀대전 관련 변수 초기화

        bEnter = false;

        //PhotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();

        //탱크를 생성하는 함수 호출
        CreateTank();
        //포톤 클라우드의 네트워크 메시지 수신을 다시 연결
        PhotonNetwork.IsMessageQueueRunning = true;

        //룸에 입장 후 기존 접속자 정보를 출력
        GetConnectPlayerCount();

        //--- CustomProperties 초기화
        InitGStateProps();
        InitSelTeamProps(); //내가 입장할 때 나를 포함한 다른 사람들에게 내가 우선 블루팀으로 등장했다고 알림
        InitReadyProps();
        //--- CustomProperties 초기화

    }//void Awake()

    // Start is called before the first frame update
    void Start()
    {
        //--- TeamSetting
        //--- 팀1 버튼 처리
        if (m_Team1ToTeam2 != null)
            m_Team1ToTeam2.onClick.AddListener(() =>
            {
                SendSelTeam("black");  //내가 블랙팀으로 이동했다고 중계함(나 포함)
            });

        if (m_Team1Ready != null)
            m_Team1Ready.onClick.AddListener(() =>
            {
                SendReady(1);
            });
        //--- 팀1 버튼 처리

        //--- 팀2 버튼 처리
        if (m_Team2ToTeam1 != null)
            m_Team2ToTeam1.onClick.AddListener(() =>
            {
                SendSelTeam("blue");  //내가 블루팀으로 이동했다고 중계함(나 포함 : RPC(AllViaServer))
            });

        if (m_Team2Ready != null)
            m_Team2Ready.onClick.AddListener(() =>
            {
                SendReady(1);
            });
        //--- 팀2 버튼 처리
        //--- TeamSetting

        if (ExitRoomBtn != null)
            ExitRoomBtn.onClick.AddListener(OnClickExitRoom);

        //로그 메시지에 출력할 문자열 생성
        string msg = "\n<color=#00ff00>["
                      + PhotonNetwork.LocalPlayer.NickName
                      + "] Connected</color>";

        //RPC 함수 호출
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, false);
        //RpcTarget.AllBuffered 옵션의 특징 : 방에서 나간 유저의 경우
        //나간 유저 버퍼에 저장돼 있던 RPC는 전달되지 않는다.
        //즉, 방에 남아 있는 유저들의 송신 되었던 버퍼 함수 내용들만 전달된다.
    }

    // Update is called once per frame
    void Update()
    {
        //게임 Update()를 돌려도 되는 상태인지 확인한다.
        if (IsGamePossible() == false)
            return;

        if(m_GameState == GameState.GS_Ready)
        {
            if(IsDifferentList() == true)
            {
                RefreshPhotonTeam();    //리스트 UI 갱신
            }
        }//if(m_GameState == GameState.GS_Ready)

        //채팅 구현 텍스트 입력
        if (Input.GetKeyDown(KeyCode.Return))
        { //엔터키를 누르면 인풋 필드 활성화
            bEnter = !bEnter;

            if(bEnter == true)
            {
                InputFdChat.gameObject.SetActive(true);
                InputFdChat.ActivateInputField(); //<-- 키보드 커서 입력 상자 쪽으로 가게 만들어 줌
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
        {//게임이 시작되었는데 아직 Team1Panel이나 m_WaitTmText 꺼지지 않았다면 확실히 끄기 위한 부분
            Team1Panel.SetActive(false);
            Team2Panel.SetActive(false);
            m_WaitTmText.gameObject.SetActive(false);
        }//if(m_GameState == GameState.GS_Playing)

        WinLossMgr.Inst.WinLossObserver(this); //한쪽팀 전멸 감시 및 승패 판정

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
        if(PhotonNetwork.PlayerList != null && PhotonNetwork.PlayerList.Length <= 1)
        {
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        }

        //지금 나가려는 탱크를 찾아서 그 탱크의
        //모든 CustomProperties를 초기화 해 주고 나가는 것이 좋다.
        //(그렇지 않으면 나갔다 즉시 방 입장시 오류 발생한다.)
        if(PhotonNetwork.LocalPlayer != null)
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

    bool IsGamePossible() //게임이 가능한 상태인지? 체크하는 함수
    {
        //나가는 타이밍에 포톤 정보들이 한플레임 먼저 사라지고
        //LoadScene()이 한플레임 늦게 호출되는 문제 해결법
        if(PhotonNetwork.CurrentRoom == null ||
           PhotonNetwork.LocalPlayer == null)
            return false; //동기화 가능한 상태일때만 업데이트를 계산해 준다.

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameState") == false ||
            PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Team1Win") == false ||
            PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Team2Win") == false)
            return false;
     
        m_GameState = ReceiveGState();
        WinLossMgr.Inst.m_Team1Win = (int)PhotonNetwork.CurrentRoom.CustomProperties["Team1Win"];
        WinLossMgr.Inst.m_Team2Win = (int)PhotonNetwork.CurrentRoom.CustomProperties["Team2Win"];

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
    // info.sender는 메시지를 보낸 플레이어의 정보를 담고 있습니다.
    //void LogMsg(string msg, PhotonMessageInfo info) //if (info.Sender.IsLocal) // 내가 보낸 메시지인 경우
    [PunRPC]
    void LogMsg(string msg, bool isChatMsg, PhotonMessageInfo info)
    {
        //로컬에서 내가 보낸 메시지인 경우만
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

        // 게임이 아직 시작되지 않은 경우는
        // 각 "유저의 별명 : 킬수 : 사망상태
        if (PhotonNetwork.CurrentRoom.IsOpen == true)
            return;

        //현재 입장한 룸에 접속한 모든 네트워크 플레이어 정보를 저장
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
                //탱크의 PlayerId가 방에 입장한 a_Player.ActorNumber 와 동일한지 판단
                if (a_TankDmg == null)
                    continue;

                if(a_TankDmg.PlayerId == a_Player.ActorNumber)
                {
                    tankDamage = a_TankDmg;
                    break;
                }
            }//foreach(GameObject a_Tank in tanks)

            if(tankDamage != null)
            {   //모든 캐릭터의 에너지바 동기화
                a_CurHp = tankDamage.currHp;
            }//if(tankDamage != null)

            string a_PrintStr = "";

            string a_sColor = "<color=Blue><size=25>";
            if(PlayerTeam == "black")
                a_sColor = "<color=Black><size=25>";

            a_PrintStr = a_sColor +
                "[" + a_Player.ActorNumber + "] " + a_Player.NickName + " "
                + curKillCount + " kill" + "</size></color>";

            if (a_CurHp <= 0)  //죽어 있을 때
                a_PrintStr += "<color=Red><size=25>" + " <Die>" + "</size></color>";

            GUILayout.Label(a_PrintStr);

            //if (a_CurHp <= 0)  //죽어 있을 때
            //{
            //    GUILayout.Label("<color=Blue><size=25>" +
            //        "[" + a_Player.ActorNumber + "] " + a_Player.NickName + " "
            //        + curKillCount + " kill" + "</size></color>"
            //        + "<color=Red><size=25>" + " <Die>" + "</size></color>");
            //}
            //else  //살아 있을 때
            //{
            //    GUILayout.Label("<color=Blue><size=25>" +
            //        "[" + a_Player.ActorNumber + "] " + a_Player.NickName + " "
            //        + curKillCount + " kill" + "</size></color>");
            //}

        }//foreach(Player a_Player in players) 

    }//private void OnGUI()

 #region ---------- 게임 상태 동기화 처리

    void InitGStateProps()
    {
        //PhotonNetwork.CurrentRoom <-- 방장이 소유하고 있는 저장공간
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

    GameState ReceiveGState()  //GameState 받아서 처리하는 부분
    {
        GameState a_RmVal = GameState.GS_Ready;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameState") == true)
            a_RmVal = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["GameState"];

        return a_RmVal;
    }

#endregion

#region --------- 팀선택 동기화 처리
    void InitSelTeamProps()
    {   //속도를 위해 버퍼를 미리 만들어 놓는다는 의미
        m_SelTeamProps.Clear();
        m_SelTeamProps.Add("MyTeam", "blue");   //방 입장시 기본적 블루팀으로 시작한다.
        PhotonNetwork.LocalPlayer.SetCustomProperties(m_SelTeamProps);                                        
        //캐릭터 별로 동기화 시키고 싶은 경우
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
        //캐릭터 별로 동기화 시키고 싶은 경우
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

    bool IsDifferentList() //true면 변화가 생겼다는 뜻 false면 같다는 뜻
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
                        return true; //해당 유저의 팀이 변경 되었다면...

                    if(a_UserData.m_IamReady != ReceiveReady(a_RefPlayer))
                        return true; //해당 Ready 상태가 변경 되었다면...

                    a_FindNode = true;
                    break;
                }
            }//foreach(GameObject a_Node  in a_UserNodeItems)

            if(a_FindNode == false)
                return true; //해당 유저가 리스트에 존재하지 않으면... 갱신 필요

        }//foreach(Player a_RefPlayer in PhotonNetwork.PlayerList)

        return false;   //일치한다는 뜻

    }//bool IsDifferentList() //true면 변화가 생겼다는 뜻 false면 같다는 뜻

    void RefreshPhotonTeam() //각 팀의 리스트뷰 UI 를 갱신해 주는 함수
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

            //팀이 뭐냐?에 따라서 스크롤 뷰를 분기 해 준다.
            if (a_TeamKind == "blue")
                a_UserNode.transform.SetParent(scrollTeam1.transform, false);
            else if (a_TeamKind == "black")
                a_UserNode.transform.SetParent(scrollTeam2.transform, false);

            //생성한 UserNodeItem에 표시하기 위한 텍스트 정보 전달
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

        //이름표 색깔 바꾸기
        DisplayUserId a_DpUserId = null;
        GameObject[] a_Tanks = GameObject.FindGameObjectsWithTag("TANK");
        foreach(GameObject tank in a_Tanks)
        {
            a_DpUserId = tank.GetComponent<DisplayUserId>();
            if (a_DpUserId == null)
                continue;

            a_DpUserId.ChangeTankNameColor(this);

        }//foreach(GameObject tank in a_Tanks)
         //이름표 색깔 바꾸기

        //--- 나의 Ready 상태에 따라서 UI 변경해 주기
        if (ReceiveReady(PhotonNetwork.LocalPlayer) == true)
        {  //내가 Ready 상태라면...
            m_Team1Ready.gameObject.SetActive(false);
            m_Team2Ready.gameObject.SetActive(false);

            m_Team1ToTeam2.gameObject.SetActive(false);
            m_Team2ToTeam1.gameObject.SetActive(false);
        }
        else  //내가 아직 Ready 상태가 아니라면...
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
        }//else  //내가 아직 Ready 상태가 아니라면...
        //--- 나의 Ready 상태에 따라서 UI 변경해 주기

    }//void RefreshPhotonTeam() //각 팀의 리스트뷰 UI 를 갱신해 주는 함수
#endregion --------- 팀선택 동기화 처리

#region --------- Ready 상태 동기화 처리

    void InitReadyProps()
    { //속도를 위해 버퍼를 미리 만들어 놓는다는 의미
        m_PlayerReady.Clear();
        m_PlayerReady.Add("IamReady", 0);   //기본적으로 아직 준비전 상태로 시작한다.
        PhotonNetwork.LocalPlayer.SetCustomProperties(m_PlayerReady);
        //캐릭터별 동기화
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
        //캐릭터별 동기화
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

    #endregion ---------  Ready 상태 동기화 처리

#region ------------ Observer Method 모음
    
    //참가자 유저 모두 Ready 버튼 눌렀는지 감시하고 게임을 시작하게 처리하는 함수
    void AllReadyObserver()
    {
        if (m_GameState != GameState.GS_Ready) //GS_Ready 상테에서만 확인한다.
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

        if(a_AllReady == true)  //모두가 준비 버튼을 누라고 기다리고 있다는 뜻
        {
            //누가 발생시켰든 동기화 시키려고 하면...
            if(m_RoundCount == 0 && PhotonNetwork.CurrentRoom.IsOpen == true)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                //게임이 시작되면 다른 유저 들어오지 못하도록 막는 부분
                //PhotonNetwork.CurrentRoom.IsVisible = false;
                //로비에서 방 목록에서도 보이지 않게 하기...
            }

            //--- 각 플레이어 PC 별로 3, 2, 1, 0 타이머 UI 표시를 위한 코드
            if(0.0f < m_GoWaitGame)  //타이머 카운트 처리
            {
                m_GoWaitGame -= Time.deltaTime;
                if(m_WaitTmText != null)
                {
                    m_WaitTmText.gameObject.SetActive(true);
                    m_WaitTmText.text = ((int)m_GoWaitGame).ToString();
                }

                //마스터 클라이언트는 각 유저의 자리배치를 해 줄 것이다.
                //총 3번만 보낸다. MasterClient가 나갈 경우를 대비해서...
                if (PhotonNetwork.IsMasterClient == true)
                if (0.0f < m_GoWaitGame && a_OldGoWait != (int)m_GoWaitGame)
                {
                    //자리 배정
                    SitPosInxMasterCtrl();
                } //1초에 한번씩 3번 자리 배정
     
                if(m_GoWaitGame <= 0.0f) //타임 아웃 되엇을 때 한번만 발생할 것임
                {//진짜 게임 시작 준비
                    m_RoundCount++;

                    Team1Panel.SetActive(false);
                    Team2Panel.SetActive(false);
                    m_WaitTmText.gameObject.SetActive(false);

                    WinLossMgr.Inst.m_CheckWinTime = 2.0f;
                    m_GoWaitGame = 0.0f;

                }//if(m_GoWaitGame <= 0.0f) //타임 아웃 되엇을 때 한번만 발생할 것임

            }//if(0.0f < m_GoWaitGame)  //타이머 카운트 처리
            //---- 각 플레이어 PC 별로 타이머 UI 표시를 위한 코드

            //게임이 시작 되었어야 하는데 아직 시작 되지 않았다면...
            if(PhotonNetwork.IsMasterClient == true) //마스터 클라이언트만 체크하고 보낸다.
            if(m_GoWaitGame <= 0.0f)
            {
                SendGState(GameState.GS_Playing);
            }

        }//if(a_AllReady == true)  //모두가 준비 버튼을 누라고 기다리고 있다는 뜻
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
