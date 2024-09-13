using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PhotonInit : MonoBehaviourPunCallbacks  //MonoBehaviour
{
    //플레이어 이름을 입력하는 UI 항목 연결
    public InputField userId;
    public Button JoinRandomRoomBtn;

    //룸 이름을 입력받을 UI 항목 연결 변수
    public InputField roomName;
    public Button CreateRoomBtn;

    //--- 룸 목록 갱신을 위한 변수들
    //RoomItem 차일드로 생성될 Parent 객체
    public GameObject scrollContents;
    //룸 목록 만큼 생성될 RoomItem 프리팹
    public GameObject roomItem;
    RoomItem[] m_RoomItemList;      //Content 하위의 차일드 목록을 찾기 위한 변수
    //--- 룸 목록 갱신을 위한 변수들

    void Awake()
    {
        //포톤 서버 접속 확인 
        //(인게임에서 빠져나온 경우가 있기 때문에...)
        if (!PhotonNetwork.IsConnected) //포톤 서버에 접속되어 있지 않으면...
        {
            //1번, 포톤 클라우드에 접속 시도
            PhotonNetwork.ConnectUsingSettings();
            //포톤 서버에 접속시도(지역 서버 접속) --> AppId 사용자 인증
        }

        //사용자 이름 설정
        userId.text = GetUserId();

        //룸 이름을 무작위로 설정
        roomName.text = "Room_" + Random.Range(0, 999).ToString("000");
    }

    // Start is called before the first frame update
    void Start()
    {
        if (JoinRandomRoomBtn != null)
            JoinRandomRoomBtn.onClick.AddListener(ClickJoinRandomRoom);

        if (CreateRoomBtn != null)
            CreateRoomBtn.onClick.AddListener(OnClickCreateRoom);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        string a_str = PhotonNetwork.NetworkClientState.ToString();
        //현재 포톤의 상태를 string으로 리턴해 주는 함수
        GUI.Label(new Rect(10, 1, 1500, 60),
                    "<color=#00ff00><size=35>" + a_str + "</size></color>");

        //if(PhotonNetwork.CurrentRoom != null)
        //    GUI.Label(new Rect(10, 50, 1500, 160),
        //               "<color=#ff0000><size=35>" + "입장한 방 이름 : " 
        //               + PhotonNetwork.CurrentRoom.Name + "</size></color>");

        //Edit 랑 Exe 파일 버전이 연결이 안되는 현상이 있을 수 있음
        //Exe 2로 테스트 해 보면 된다.
    }

    //2번, ConnectUsingSetting() 함수 호출에 대한 서버 접속이 성고하면
    //호출되는 콜백 함수
    //PhotonNetwork.LeaveRoom(); 으로 방을 떠날 때도 로비로 나오면서 이 함수가 
    //자동으로 호출된다.
    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 접속 완료");
        //단순 포톤 서버 접속만 된 상태(ConnectToMaster)

        //3번, 규모가 작은 게임에서는 서버로비가 보통 하나이고...
        //대형 게임인 경우 상급자로비, 중급자로비, 초보자로비처럼
        //로비가 여러개일 수 있다.
        PhotonNetwork.JoinLobby(); //포톤에서 제공해 주는 가상의 로비에 접속 시도
    }

    //4번, PhotonNetwork.JoinLobby() 성공시 호출되는 로비 접속 콜백함수
    public override void OnJoinedLobby()
    {
        Debug.Log("로비 접속 완료");

        userId.text = GetUserId();

        ////무작위 방으로 입장
        //PhotonNetwork.JoinRandomRoom();
    }

    public void ClickJoinRandomRoom()  //3번 방 입장 버튼을 클릭했을 때
    {
        //로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = userId.text;

        //플레이어 이름을 저장
        PlayerPrefs.SetString("USER_ID", userId.text);

        //무작위 방으로 입장
        PhotonNetwork.JoinRandomRoom();
    }

    //PhotonNetwork.JoinRandomRoom(); 함수가 실패한 경우 호출되는 오버라이드 함수
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("랜덤 방 참가 실패 (참가할 방이 존재하지 않습니다.)");

        //방을 생서하면서 들어감
        //생성할 룸의 조건부터 설정
        RoomOptions roomOptions = new RoomOptions();    //using Photon.Realtime;
        roomOptions.IsVisible = true;   //로비에서 룸의 노출 여부
        roomOptions.MaxPlayers = 8;     //룸에 입장할 수 있는 최대 접속자 수

        //지정한 조건에 맞는 룸생성 함수
        PhotonNetwork.CreateRoom("MyRoom", roomOptions);
        //랜덤하게 들어갈 방이 하나도 없으면 내가 방을 만들면서 입장하겠다는 개념
    }

    //PhotonNetwork.CreateRoom() 함수가 성공하면 자동으로 호출되는 함수
    //PhotonNetwork.JoinRoom() 함수가 성공해도 자동으로 호출되는 함수
    //PhotonNetwork.JoinRandomRoom(); 함수가 성공해도 자동으로 호출되는 함수
    public override void OnJoinedRoom()
    {  //서버 역할인 경우 5번 : 방입장, 클라이언트 역할인 경우 4번 : 방입장
        Debug.Log("방 참가 완료");

        ////탱크를 생성하는 함수 호출
        //CreateTank();

        StartCoroutine(this.LoadBattleField());
    }

    //룸 씬으로 이동하는 코루틴 함수
    IEnumerator LoadBattleField()
    {
        //씬을 이용하는 동안 포톤 클라우드 서버로부터 네트워크 메시지 수신 중단
        PhotonNetwork.IsMessageQueueRunning = false;

        Time.timeScale = 1.0f;  //게임에 들어갈 때는 원래 속도로...

        //백그라운드로 씬 로딩
        AsyncOperation ao = SceneManager.LoadSceneAsync("scBattleField");
        //while (!ao.isDone)
        //{
        //    Debug.Log(ao.progress); //로딩 진행 상태 출력
        //}
        yield return ao;
    }

    //Make Room 버튼 클릭 시 호출될 함수
    void OnClickCreateRoom()
    {
        string _roomName = roomName.text;
        //룸 이름이 없거나 null 일 경우 룸 이름 저장
        if (string.IsNullOrEmpty(roomName.text))
        {
            _roomName = "Room_" + Random.Range(0, 999).ToString("000");
        }

        //로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = userId.text;
        //플레이어 이름을 저장
        PlayerPrefs.SetString("USER_ID", userId.text);

        //생성할 룸의 조건 설정
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;      //입장 가능 여부
        roomOptions.IsVisible = true;   //로비에서 룸의 노출 여부
        roomOptions.MaxPlayers = 8;     //룸에 입장할 수 있는 최대 접속자 수

        //지정한 조건에 맞는 룸 생성 함수
        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
        //TypedLobby.Default : 어느 로비에 방을 만들건지?

    }//void OnClickCreateRoom()

    //PhotonNetwork.CreateRoom() 함수 호출이 실패 하면 호출되는 함수
    //(같은 이름의 방이 이미 있을 때 실패함)
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 만들기 실패");
        //주로 같은 이름의 방이 존재할 때 룸 생성 에러가 발생된다.
        Debug.Log(returnCode.ToString());   //오류코드(ErrorCode 클래스)
        Debug.Log(message); //오류 메시지
    }

    //생성된 룸 목록이 변경 되었을 때 호출되는 오버라이드 함수
    //방 리스트 갱신은 포톤 클라우드 로비에서만 가능하다.
    //<이 함수가 호출되는 상황들 (방 정보 갱신이 필요한 상황들)>
    //1, 내가 로비로 진입할 때 OnRoomListUpdate() 함수가 호출되면서 방목록을 보내 줌
    //2, 누군가 방을 새로 만들거나 방이 파괴될 때 OnRoomListUpdate() 함수로 갱신될 방정보를 보내 줌
    //   A가 로비에서 대기하고 있고 B가 방을 만들고 들어가면
    //   OnRoomListUpdate() 가 로비에서 대기하고 있었던 A쪽으로 호출된다.
    //   B가 방을 만들면서 들어갈 때는 roomList[i].RemovedFromList == false 가 되고,
    //   B가 방을 떠나면서 방이 제거되야 할 때 roomList[i].RemovedFromList == true가 된다.
    //3, A가 로그아웃(포톤 서버에 접속 끊기) 했다가 다시 로비까지 들어올 때도 
    //   OnRoomListUpdate() 함수를 받게 된다.
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        m_RoomItemList = scrollContents.transform.GetComponentsInChildren<RoomItem>(true);

        int roomCnt = roomList.Count;
        int a_ArrIdx = 0;

        for (int i = 0; i < roomCnt; i++)
        {
            a_ArrIdx = MyFindIdx(m_RoomItemList, roomList[i]);

            if (roomList[i].RemovedFromList == false)
            {
                if (a_ArrIdx < 0)
                {
                    // 새로 생성
                    Debug.Log("새로 생성");
                    GameObject a_Room = Instantiate(roomItem);
                    a_Room.transform.SetParent(scrollContents.transform, false);

                    RoomItem a_RData = a_Room.GetComponent<RoomItem>();
                    a_RData.roomName = roomList[i].Name;
                    a_RData.connectPlayer = roomList[i].PlayerCount;
                    a_RData.maxPlayer = roomList[i].MaxPlayers;

                    a_RData.DispRoomData(roomList[i].IsOpen);
                }
                else
                {
                    Debug.Log("갱신");
                    // 이미 생성된 RoomItem이면 갱신
                    m_RoomItemList[a_ArrIdx].roomName = roomList[i].Name;
                    m_RoomItemList[a_ArrIdx].connectPlayer = roomList[i].PlayerCount;
                    m_RoomItemList[a_ArrIdx].maxPlayer = roomList[i].MaxPlayers;

                    m_RoomItemList[a_ArrIdx].DispRoomData(roomList[i].IsOpen);
                }
            }
            else
            {
                // 방파괴 & 리스트뷰에서 제거
                if (0 <= a_ArrIdx)
                {
                    MyDestroy(m_RoomItemList, roomList[i]);
                }
            }
        }
    }

    public void OnClickRoomItem(string roomName)
    {
        PhotonNetwork.LocalPlayer.NickName = userId.text;

        //플레이어 이름을 저장
        PlayerPrefs.SetString("USER_ID", userId.text);

        //해당 룸으로 입장
        PhotonNetwork.JoinRoom(roomName);
    }


    ////탱크를 생성하는 함수
    //void CreateTank()
    //{
    //    float pos = Random.Range(-100.0f, 100.0f);
    //    PhotonNetwork.Instantiate("Tank", new Vector3(pos, 20.0f, pos), Quaternion.identity, 0);
    //}

    //로컬에 저장된 플레이어 이름을 반환하거나 생성하는 함수
    string GetUserId()
    {
        string userId = PlayerPrefs.GetString("USER_ID");

        if (string.IsNullOrEmpty(userId))
        {
            userId = "USER_" + Random.Range(0, 999).ToString("000");
        }

        return userId;
    }


    int MyFindIdx(RoomItem[] a_RItemList, RoomInfo a_RoomInfo)
    {
        if (a_RItemList == null)
            return -1;

        if (a_RItemList.Length == 0)
            return -1;

        for (int i = 0; i< a_RItemList.Length; i++)
        {
            if (a_RItemList[i].roomName == a_RoomInfo.Name)
            {
                return i;
            }
        }

        return -1;

    }


    void MyDestroy(RoomItem[] a_RItemList, RoomInfo a_RoomInfo)
    {
        if (a_RItemList == null)
            return;

        if (a_RItemList.Length == 0)
            return;

        for (int i = 0; i < a_RItemList.Length; i++)
        {
            if (a_RItemList[i].roomName == a_RoomInfo.Name)
            {
                Destroy(a_RItemList[i].gameObject);
            }
        }
    }

}
