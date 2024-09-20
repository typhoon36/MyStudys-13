using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PhotonInit : MonoBehaviourPunCallbacks  //MonoBehaviour
{
    //�÷��̾� �̸��� �Է��ϴ� UI �׸� ����
    public InputField userId;
    public Button JoinRandomRoomBtn;

    //�� �̸��� �Է¹��� UI �׸� ���� ����
    public InputField roomName;
    public Button CreateRoomBtn;

    //--- �� ��� ������ ���� ������
    //RoomItem ���ϵ�� ������ Parent ��ü
    public GameObject scrollContents;
    //�� ��� ��ŭ ������ RoomItem ������
    public GameObject roomItem;
    RoomItem[] m_RoomItemList;      //Content ������ ���ϵ� ����� ã�� ���� ����
    //--- �� ��� ������ ���� ������

    void Awake()
    {
        //���� ���� ���� Ȯ�� 
        //(�ΰ��ӿ��� �������� ��찡 �ֱ� ������...)
        if(!PhotonNetwork.IsConnected) //���� ������ ���ӵǾ� ���� ������...
        {
            //1��, ���� Ŭ���忡 ���� �õ�
            PhotonNetwork.ConnectUsingSettings();
            //���� ������ ���ӽõ�(���� ���� ����) --> AppId ����� ����
        }

        //����� �̸� ����
        userId.text = GetUserId();

        //�� �̸��� �������� ����
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
        //���� ������ ���¸� string���� ������ �ִ� �Լ�
        GUI.Label(new Rect(10, 1, 1500, 60),
                    "<color=#00ff00><size=35>" + a_str + "</size></color>");

        //if(PhotonNetwork.CurrentRoom != null)
        //    GUI.Label(new Rect(10, 50, 1500, 160),
        //               "<color=#ff0000><size=35>" + "������ �� �̸� : " 
        //               + PhotonNetwork.CurrentRoom.Name + "</size></color>");

        //Edit �� Exe ���� ������ ������ �ȵǴ� ������ ���� �� ����
        //Exe 2�� �׽�Ʈ �� ���� �ȴ�.
    }

    //2��, ConnectUsingSetting() �Լ� ȣ�⿡ ���� ���� ������ �����ϸ�
    //ȣ��Ǵ� �ݹ� �Լ�
    //PhotonNetwork.LeaveRoom(); ���� ���� ���� ���� �κ�� �����鼭 �� �Լ��� 
    //�ڵ����� ȣ��ȴ�.
    public override void OnConnectedToMaster()
    {
        Debug.Log("���� ���� �Ϸ�");
        //�ܼ� ���� ���� ���Ӹ� �� ����(ConnectToMaster)

        //3��, �Ը� ���� ���ӿ����� �����κ� ���� �ϳ��̰�...
        //���� ������ ��� ����ڷκ�, �߱��ڷκ�, �ʺ��ڷκ�ó��
        //�κ� �������� �� �ִ�.
        PhotonNetwork.JoinLobby(); //���濡�� ������ �ִ� ������ �κ� ���� �õ�
    }

    //4��, PhotonNetwork.JoinLobby() ������ ȣ��Ǵ� �κ� ���� �ݹ��Լ�
    public override void OnJoinedLobby()
    { 
        Debug.Log("�κ� ���� �Ϸ�");

        userId.text = GetUserId();

        ////������ ������ ����
        //PhotonNetwork.JoinRandomRoom();
    }

    public void ClickJoinRandomRoom()  //3�� �� ���� ��ư�� Ŭ������ ��
    {
        //���� �÷��̾��� �̸��� ����
        PhotonNetwork.LocalPlayer.NickName = userId.text;

        //�÷��̾� �̸��� ����
        PlayerPrefs.SetString("USER_ID", userId.text);

        //������ ������ ����
        PhotonNetwork.JoinRandomRoom();
    }

    //PhotonNetwork.JoinRandomRoom(); �Լ��� ������ ��� ȣ��Ǵ� �������̵� �Լ�
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("���� �� ���� ���� (������ ���� �������� �ʽ��ϴ�.)");

        //���� �����ϸ鼭 ��
        //������ ���� ���Ǻ��� ����
        RoomOptions roomOptions = new RoomOptions();    //using Photon.Realtime;
        roomOptions.IsVisible = true;   //�κ񿡼� ���� ���� ����
        roomOptions.MaxPlayers = 8;     //�뿡 ������ �� �ִ� �ִ� ������ ��

        //������ ���ǿ� �´� ����� �Լ�
        PhotonNetwork.CreateRoom("MyRoom", roomOptions);
        //�����ϰ� �� ���� �ϳ��� ������ ���� ���� ����鼭 �����ϰڴٴ� ����
    }

    //PhotonNetwork.CreateRoom() �Լ��� �����ϸ� �ڵ����� ȣ��Ǵ� �Լ�
    //PhotonNetwork.JoinRoom() �Լ��� �����ص� �ڵ����� ȣ��Ǵ� �Լ�
    //PhotonNetwork.JoinRandomRoom(); �Լ��� �����ص� �ڵ����� ȣ��Ǵ� �Լ�
    public override void OnJoinedRoom()
    {  //���� ������ ��� 5�� : ������, Ŭ���̾�Ʈ ������ ��� 4�� : ������
        Debug.Log("�� ���� �Ϸ�");

        ////��ũ�� �����ϴ� �Լ� ȣ��
        //CreateTank();

        StartCoroutine(this.LoadBattleField());
    }

    //�� ������ �̵��ϴ� �ڷ�ƾ �Լ�
    IEnumerator LoadBattleField()
    {
        //���� �̿��ϴ� ���� ���� Ŭ���� �����κ��� ��Ʈ��ũ �޽��� ���� �ߴ�
        PhotonNetwork.IsMessageQueueRunning = false;

        Time.timeScale = 1.0f;  //���ӿ� �� ���� ���� �ӵ���...

        //��׶���� �� �ε�
        AsyncOperation ao = SceneManager.LoadSceneAsync("scBattleField");
        //while (!ao.isDone)
        //{
        //    Debug.Log(ao.progress); //�ε� ���� ���� ���
        //}
        yield return ao;
    }

    //Make Room ��ư Ŭ�� �� ȣ��� �Լ�
    void OnClickCreateRoom()
    {
        string _roomName = roomName.text;
        //�� �̸��� ���ų� null �� ��� �� �̸� ����
        if(string.IsNullOrEmpty(roomName.text))
        {
            _roomName = "Room_" + Random.Range(0, 999).ToString("000");
        }

        //���� �÷��̾��� �̸��� ����
        PhotonNetwork.LocalPlayer.NickName = userId.text;
        //�÷��̾� �̸��� ����
        PlayerPrefs.SetString("USER_ID", userId.text);

        //������ ���� ���� ����
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;      //���� ���� ����
        roomOptions.IsVisible = true;   //�κ񿡼� ���� ���� ����
        roomOptions.MaxPlayers = 8;     //�뿡 ������ �� �ִ� �ִ� ������ ��

        //������ ���ǿ� �´� �� ���� �Լ�
        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
        //TypedLobby.Default : ��� �κ� ���� �������?

    }//void OnClickCreateRoom()

    //PhotonNetwork.CreateRoom() �Լ� ȣ���� ���� �ϸ� ȣ��Ǵ� �Լ�
    //(���� �̸��� ���� �̹� ���� �� ������)
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("�� ����� ����");
        //�ַ� ���� �̸��� ���� ������ �� �� ���� ������ �߻��ȴ�.
        Debug.Log(returnCode.ToString());   //�����ڵ�(ErrorCode Ŭ����)
        Debug.Log(message); //���� �޽���
    }

    //������ �� ����� ���� �Ǿ��� �� ȣ��Ǵ� �������̵� �Լ�
    //�� ����Ʈ ������ ���� Ŭ���� �κ񿡼��� �����ϴ�.
    //<�� �Լ��� ȣ��Ǵ� ��Ȳ�� (�� ���� ������ �ʿ��� ��Ȳ��)>
    //1, ���� �κ�� ������ �� OnRoomListUpdate() �Լ��� ȣ��Ǹ鼭 ������ ���� ��
    //2, ������ ���� ���� ����ų� ���� �ı��� �� OnRoomListUpdate() �Լ��� ���ŵ� �������� ���� ��
    //   A�� �κ񿡼� ����ϰ� �ְ� B�� ���� ����� ����
    //   OnRoomListUpdate() �� �κ񿡼� ����ϰ� �־��� A������ ȣ��ȴ�.
    //   B�� ���� ����鼭 �� ���� roomList[i].RemovedFromList == false �� �ǰ�,
    //   B�� ���� �����鼭 ���� ���ŵǾ� �� �� roomList[i].RemovedFromList == true�� �ȴ�.
    //3, A�� �α׾ƿ�(���� ������ ���� ����) �ߴٰ� �ٽ� �κ���� ���� ���� 
    //   OnRoomListUpdate() �Լ��� �ް� �ȴ�.
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        m_RoomItemList = scrollContents.transform.GetComponentsInChildren<RoomItem>(true);

        int roomCount = roomList.Count;
        int a_ArrIdx = 0;
        for(int i = 0; i < roomCount; i++)
        {
            a_ArrIdx = MyFindIndex(m_RoomItemList, roomList[i]);

            if (roomList[i].RemovedFromList == false)
            { //���� ���� �����ϰų�, �������� ������ ��� �ϴ� ��Ȳ
                if(a_ArrIdx < 0)
                {  //���� ���� �����ϰų�, �������� ������ ��� �ϴ� ��Ȳ
                    // ��ũ�� �信 �ٿ��� ���ο� �� ������Ʈ�� ���� ������ ��� ��
                    //--- ���ο� �� ������Ʈ ���� ����
                    GameObject room = Instantiate(roomItem) as GameObject; 
                    //������ RoomItem �������� Parent�� ����
                    room.transform.SetParent(scrollContents.transform, false);
                    //������ RoomItem�� ǥ���ϱ� ���� �ؽ�Ʈ ���� ����
                    RoomItem roomData = room.GetComponent<RoomItem>();
                    roomData.roomName = roomList[i].Name;
                    roomData.connectPlayer = roomList[i].PlayerCount;
                    roomData.maxPlayer = roomList[i].MaxPlayers;

                    //�ؽ�Ʈ ������ ǥ��
                    roomData.DispRoomData(roomList[i].IsOpen);
                }
                else
                {  //�ش� ���� ����Ʈ �信 �����ϸ� �������� ������ ��� �Ѵٴ� �ǹ�
                    //�⺻ �� ������ ����
                    m_RoomItemList[a_ArrIdx].roomName = roomList[i].Name;
                    m_RoomItemList[a_ArrIdx].connectPlayer = roomList[i].PlayerCount;
                    m_RoomItemList[a_ArrIdx].maxPlayer = roomList[i].MaxPlayers;

                    //�ؽ�Ʈ ������ ǥ��
                    m_RoomItemList[a_ArrIdx].DispRoomData(roomList[i].IsOpen);
                }
            }//if (roomList[i].RemovedFromList == false)
            else //if(roomList[i].RemovedFromList == true)
            { //���� �ı��Ǿ ����Ʈ�信�� RoomItem�� ������ ��� �ϴ� ��Ȳ
                if(0 <= a_ArrIdx)
                {
                    MyDestroy(m_RoomItemList, roomList[i]);
                    //�� �� ������ ���� �ִ� ����Ʈ�� ����� ��� ����
                }
            }
        }// for(int i = 0; i < roomCount; i++)
    }//public override void OnRoomListUpdate(List<RoomInfo> roomList)

    int MyFindIndex(RoomItem[] a_RmItemList, RoomInfo a_RoomInfo)
    {
        if(a_RmItemList == null)
            return -1;

        if(a_RmItemList.Length <= 0)
            return -1;

        for(int i = 0; i < a_RmItemList.Length; i++)
        {
            if (a_RmItemList[i].roomName == a_RoomInfo.Name)
            {
                return i;
            }
        }

        return -1;
    }//int MyFindIndex(RoomItem[] a_RmItemList, RoomInfo a_RoomInfo)

    void MyDestroy(RoomItem[] a_RmItemList, RoomInfo a_RoomInfo)
    {
        if (a_RmItemList == null)
            return;

        if(a_RmItemList.Length <= 0)
            return;

        for(int i = 0; i < a_RmItemList.Length; i++)
        {
            if (a_RmItemList[i].roomName == a_RoomInfo.Name)
            {
                Destroy(a_RmItemList[i].gameObject);
            }
        }
    }//void MyDestroy(RoomItem[] a_RmItemList, RoomInfo a_RoomInfo)

    public void OnClickRoomItem(string roomName)
    {
        //���� �÷��̾��� �̸��� ����
        PhotonNetwork.LocalPlayer.NickName = userId.text;
        //�÷��̾� �̸��� ����
        PlayerPrefs.SetString("USER_ID", userId.text);

        //���ڷ� ���޵� �̸��� �ش��ϴ� ������ ����
        PhotonNetwork.JoinRoom(roomName);
    }

    ////��ũ�� �����ϴ� �Լ�
    //void CreateTank()
    //{
    //    float pos = Random.Range(-100.0f, 100.0f);
    //    PhotonNetwork.Instantiate("Tank", new Vector3(pos, 20.0f, pos), Quaternion.identity, 0);
    //}

    //���ÿ� ����� �÷��̾� �̸��� ��ȯ�ϰų� �����ϴ� �Լ�
    string GetUserId()
    {
        string userId = PlayerPrefs.GetString("USER_ID");

        if(string.IsNullOrEmpty(userId))
        {
            userId = "USER_" + Random.Range(0, 999).ToString("000");
        }

        return userId;
    }

}
