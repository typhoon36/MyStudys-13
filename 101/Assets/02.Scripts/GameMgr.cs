using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMgr : MonoBehaviourPunCallbacks
{
    public static bool isFocus = true;
    //이 클라이언트 윈도우가 마우스 포커스를 가지고 있는지? 확인하는 변수

    //텍스트
    public Text UserCnt_Text;
    public Button Exit_Btn;

    //# 접속 로그
    public Text Lob_Txt;
    //# 포톤뷰
    PhotonView pv;

    //# 채팅 
    public InputField Chat_IF;
    public static bool bChat = false;

    void Awake()
    {
        bChat = false;

        // PhotonView 초기화
        pv = GetComponent<PhotonView>();

        //탱크를 생성하는 함수 호출
        CreateTank();
        //포톤 클라우드의 네트워크 메시지 수신을 다시 연결
        PhotonNetwork.IsMessageQueueRunning = true;

        //플레이어 접속자 수 표시
        GetConnecPlayerCnt();
    }

    // Start is called before the first frame update
    void Start()
    {
        //나가기 버튼 클릭 이벤트
        Exit_Btn.onClick.AddListener(OnclickExit_Btn);

        //메시지에 출력 문자열
        string msg = "\n<color=#00ff00>[" +
            PhotonNetwork.LocalPlayer.NickName +
            "] Connected</color>";

        //RPC 전송
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, PhotonNetwork.LocalPlayer.ActorNumber, false);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGamePossible() == false)
            return;

        //채팅 구현
        if (Input.GetKeyDown(KeyCode.Return))
        {
            //엔터키를 눌렀을때
            bChat = !bChat;

            if (bChat)
            {
                Chat_IF.gameObject.SetActive(true);
                Chat_IF.ActivateInputField();
            }
            else
            {
                Chat_IF.gameObject.SetActive(false);

                if (string.IsNullOrEmpty(Chat_IF.text.Trim()) == false)
                {
                    BroadcastingChat();
                }
            }
        }
    }

    void BroadcastingChat()
    {
        string msg = "\n<color=#ffff00>[" + // 자신이 입력한 메시지는 노란색으로 표시
            PhotonNetwork.LocalPlayer.NickName +
            "] " + Chat_IF.text + "</color>";

        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, PhotonNetwork.LocalPlayer.ActorNumber, true);

        Chat_IF.text = "";
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

    void GetConnecPlayerCnt()
    {
        Room Cur_Room = PhotonNetwork.CurrentRoom;

        //현재 룸 접속자 수 와 최대 접속자 수를 문자열로 text UI에 표시
        UserCnt_Text.text = Cur_Room.PlayerCount.ToString()
            + "/"
            + Cur_Room.MaxPlayers.ToString();
    }

    public override void OnPlayerEnteredRoom(Player N_Player)
    {
        GetConnecPlayerCnt();
    }

    //플레이어가 룸을 떠났을 때(접속 안되는것도 포함) 호출되는 함수
    public override void OnPlayerLeftRoom(Player Other_PL)
    {
        GetConnecPlayerCnt();
    }

    //나가기 버튼 클릭 이벤트
    public void OnclickExit_Btn()
    {
        string msg = "\n<color=#ff0000>[" +
            PhotonNetwork.LocalPlayer.NickName +
            "] Disconnected</color>";

        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, PhotonNetwork.LocalPlayer.ActorNumber, false);

        // 마지막 플레이어가 방을 나가면 방이 자동으로 삭제되도록 설정
        if (PhotonNetwork.PlayerList != null && PhotonNetwork.PlayerList.Length <= 1)
        {
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        }

        // 룸을 떠날 때 탱크를 찾고 그 탱크의 customProperties를 초기화 시킨다.
        if (PhotonNetwork.LocalPlayer != null)
            PhotonNetwork.LocalPlayer.CustomProperties.Clear();

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("scLobby");
    }

    public static bool IsGamePossible()
    {
        //# Loadscnene()이 프레임 늦는 현상 해결(나갈때 포톤정보들이 한프레임 일찍 사라짐)
        if (PhotonNetwork.CurrentRoom == null ||
            PhotonNetwork.LocalPlayer == null)
            return false;

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

    List<string> m_ChatList = new List<string>();

    [PunRPC]
    void LogMsg(string a_Msg, int senderActorNumber, bool isChatMessage)
    {
        string formattedMsg = a_Msg;

        if (isChatMessage)
        {
            // 자신이 보낸 메시지인지 확인
            if (senderActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                // 다른 사람이 보낸 메시지는 하얀색으로 표시
                formattedMsg = a_Msg.Replace("#ffff00", "#ffffff");
            }
        }

        m_ChatList.Add(formattedMsg);
        if (20 < m_ChatList.Count)
            m_ChatList.RemoveAt(0);
        Lob_Txt.text = "";

        for (int i = 0; i < m_ChatList.Count; i++)
        {
            Lob_Txt.text += m_ChatList[i];
        }
    }
}
