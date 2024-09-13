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
    //�� Ŭ���̾�Ʈ �����찡 ���콺 ��Ŀ���� ������ �ִ���? Ȯ���ϴ� ����

    //�ؽ�Ʈ
    public Text UserCnt_Text;
    public Button Exit_Btn;

    //# ���� �α�
    public Text Lob_Txt;
    //# �����
    PhotonView pv;

    //# ä�� 
    public InputField Chat_IF;
    public static bool bChat = false;

    void Awake()
    {
        bChat = false;

        // PhotonView �ʱ�ȭ
        pv = GetComponent<PhotonView>();

        //��ũ�� �����ϴ� �Լ� ȣ��
        CreateTank();
        //���� Ŭ������ ��Ʈ��ũ �޽��� ������ �ٽ� ����
        PhotonNetwork.IsMessageQueueRunning = true;

        //�÷��̾� ������ �� ǥ��
        GetConnecPlayerCnt();
    }

    // Start is called before the first frame update
    void Start()
    {
        //������ ��ư Ŭ�� �̺�Ʈ
        Exit_Btn.onClick.AddListener(OnclickExit_Btn);

        //�޽����� ��� ���ڿ�
        string msg = "\n<color=#00ff00>[" +
            PhotonNetwork.LocalPlayer.NickName +
            "] Connected</color>";

        //RPC ����
        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, PhotonNetwork.LocalPlayer.ActorNumber, false);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGamePossible() == false)
            return;

        //ä�� ����
        if (Input.GetKeyDown(KeyCode.Return))
        {
            //����Ű�� ��������
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
        string msg = "\n<color=#ffff00>[" + // �ڽ��� �Է��� �޽����� ��������� ǥ��
            PhotonNetwork.LocalPlayer.NickName +
            "] " + Chat_IF.text + "</color>";

        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, PhotonNetwork.LocalPlayer.ActorNumber, true);

        Chat_IF.text = "";
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

    void GetConnecPlayerCnt()
    {
        Room Cur_Room = PhotonNetwork.CurrentRoom;

        //���� �� ������ �� �� �ִ� ������ ���� ���ڿ��� text UI�� ǥ��
        UserCnt_Text.text = Cur_Room.PlayerCount.ToString()
            + "/"
            + Cur_Room.MaxPlayers.ToString();
    }

    public override void OnPlayerEnteredRoom(Player N_Player)
    {
        GetConnecPlayerCnt();
    }

    //�÷��̾ ���� ������ ��(���� �ȵǴ°͵� ����) ȣ��Ǵ� �Լ�
    public override void OnPlayerLeftRoom(Player Other_PL)
    {
        GetConnecPlayerCnt();
    }

    //������ ��ư Ŭ�� �̺�Ʈ
    public void OnclickExit_Btn()
    {
        string msg = "\n<color=#ff0000>[" +
            PhotonNetwork.LocalPlayer.NickName +
            "] Disconnected</color>";

        pv.RPC("LogMsg", RpcTarget.AllBuffered, msg, PhotonNetwork.LocalPlayer.ActorNumber, false);

        // ������ �÷��̾ ���� ������ ���� �ڵ����� �����ǵ��� ����
        if (PhotonNetwork.PlayerList != null && PhotonNetwork.PlayerList.Length <= 1)
        {
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        }

        // ���� ���� �� ��ũ�� ã�� �� ��ũ�� customProperties�� �ʱ�ȭ ��Ų��.
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
        //# Loadscnene()�� ������ �ʴ� ���� �ذ�(������ ������������ �������� ���� �����)
        if (PhotonNetwork.CurrentRoom == null ||
            PhotonNetwork.LocalPlayer == null)
            return false;

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

    List<string> m_ChatList = new List<string>();

    [PunRPC]
    void LogMsg(string a_Msg, int senderActorNumber, bool isChatMessage)
    {
        string formattedMsg = a_Msg;

        if (isChatMessage)
        {
            // �ڽ��� ���� �޽������� Ȯ��
            if (senderActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                // �ٸ� ����� ���� �޽����� �Ͼ������ ǥ��
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
