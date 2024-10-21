using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum PacketType
{
    //--- Title
    Login,          //�α��� ��û
    CreateAccount,  //���� ���� ��û
    //--- Title

    //--- Lobby
    GetRankingList, //��ŷ �޾ƿ���
    NickUpdate,     //�г��Ӱ���
    ClearSave,      //������ ����� ���� �ʱ�ȭ �ϱ� 
    //--- Lobby

    //--- InGame
    BestScore,      //�ְ�����
    UserGold,       //�������
    InfoUpdate,     //������������, ����������
    FloorUpdate,    //������
    //--- InGame

    //--- Store
    BuyRequest,     //��ǰ ���� ��û
    //--- Store
}

[System.Serializable]
public class ItemList
{
    public int[] SkList;
}

[System.Serializable]
public class FloorInfo   //�� ���� �ε� 
{
    public int CurFloor;
    public int BestFloor;
}

[System.Serializable]
public class SvRespon    //ServerResponse
{
    public string nick_name;
    public int best_score;
    public int game_gold;
    public string floor_info;  //�� �ʵ�� ���ڿ��� �����ϰ�, �߰������� �Ľ��� �ʿ���
    public string info;        //�� �ʵ�� ���ڿ��� �����ϰ�, �߰������� �Ľ��� �ʿ���
}

public class NetworkMgr : G_Singleton<NetworkMgr>
{
    //--- ������ ������ ��Ŷ ó���� ť ���� ����
    //[HideInInspector] public bool isNetworkLock = false; //Network ��� ���� ���� ����
    [HideInInspector] public float m_NetWaitTimer = 0.0f;  //Network ��� ���� ���� ����
    List<PacketType> m_PacketBuff = new List<PacketType>();
    //--- ������ ������ ��Ŷ ó���� ť ���� ����

    //3�� ���� ������ ������ ���� ��Ŷ ó�� �����ϵ���...
    public const float m_Timeout = 3.0f;

    //--- Scene�� ������Ʈ ��ü
    [HideInInspector] public TitleNetCoroutine TitleNetCom;
    //[HideInInspector] public LobbyNetCoroutine LobbyNetCom;
    //[HideInInspector] public InGameNetCoroutine InGameNetCom;
    //[HideInInspector] public StoreNetCoroutine StoreNetCom;
    //--- Scene�� ������Ʈ ��ü

    //--- �α���, �������� �Ű������� �Ѱ� ���� �ӽú���
    [HideInInspector] public string m_IdStrBuff = "";
    [HideInInspector] public string m_PwStrBuff = "";
    [HideInInspector] public string m_NickStrBuff = "";
    //--- �α���, �������� �Ű������� �Ѱ� ���� �ӽú���

    //--- �г��� ���濡 �ʿ��� ������
    [HideInInspector] public string m_NickCgBuff = "";
    //[HideInInspector] public ConfigBox m_RefCfgBox = null;
    //--- �г��� ���濡 �ʿ��� ������

    protected override void Init()  //Awake() �Լ� ��� ���
    {
        base.Init(); //�θ��ʿ� �ִ� Init() �Լ� ȣ��

        TitleNetCom  = gameObject.AddComponent<TitleNetCoroutine>();   //������Ʈ �߰�
        //LobbyNetCom  = gameObject.AddComponent<LobbyNetCoroutine>();   //������Ʈ �߰�
        //InGameNetCom = gameObject.AddComponent<InGameNetCoroutine>();  //������Ʈ �߰�
        //StoreNetCom  = gameObject.AddComponent<StoreNetCoroutine>();
    }

    public void ReadyNetworkMgr(MonoBehaviour a_CurMgr)  //�ʱ�ȭ�� �ѹ� ȣ��
    {
        //as �����ڴ� �־��� ��ü�� ������ Ÿ������ ĳ������ �������� Ȯ���ϰ�,
        //�����ϸ� �ش� Ÿ������ ��ȯ�ϸ�, �Ұ����ϸ� null�� ��ȯ�մϴ�.
        Title_Mgr a_Tm = a_CurMgr as Title_Mgr;
        if (a_Tm != null)
        {
            TitleNetCom.TitleStart(a_Tm);
        }

        //LobbyMgr a_Lm = a_CurMgr as LobbyMgr;
        //if (a_Lm != null)
        //{
        //    LobbyNetCom.LobbyStart(a_Lm);
        //}

        //GameMgr a_Gm = a_CurMgr as GameMgr;
        //if (a_Gm != null)
        //{
        //    InGameNetCom.GameStart(a_Gm);
        //}

        //DragAndDropMgr a_DAD = a_CurMgr as DragAndDropMgr;
        //if (a_DAD != null)
        //{
        //    StoreNetCom.StoreStart(a_DAD);
        //}
    }

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    // Update is called once per frame
    void Update()
    {
        if (0.0f <= m_NetWaitTimer)
            m_NetWaitTimer -= Time.unscaledDeltaTime;

        //if (isNetworkLock == false)  //���� ��Ŷ ó�� ���� ���°� �ƴϸ�...
        if(m_NetWaitTimer <= 0.0f) //���� ��Ŷ ó�� ���� ���°� �ƴϸ�...
        {
            if (0 < m_PacketBuff.Count)  //��� ��Ŷ�� �����Ѵٸ�...
            {
                Req_Network();
            }
        }//if(m_NetWaitTimer <= 0.0f) //���� ��Ŷ ó�� ���� ���°� �ƴϸ�...
    }

    void Req_Network()  //RequestNetwork
    {
        //--- Title
        if (m_PacketBuff[0] == PacketType.Login)   //�α��� ��û
            StartCoroutine(TitleNetCom.LoginCo(m_IdStrBuff, m_PwStrBuff));
        else if (m_PacketBuff[0] == PacketType.CreateAccount)  //���� ���� ��û
            StartCoroutine(TitleNetCom.CreateActCo(m_IdStrBuff, m_PwStrBuff, m_NickStrBuff));
        //--- Title
        ////--- Lobby
        //else if (m_PacketBuff[0] == PacketType.GetRankingList)
        //    StartCoroutine(LobbyNetCom.GetRankListCo());
        //else if (m_PacketBuff[0] == PacketType.NickUpdate)
        //    StartCoroutine(LobbyNetCom.NickChangeCo(m_NickCgBuff, m_RefCfgBox));
        //else if (m_PacketBuff[0] == PacketType.ClearSave)
        //    StartCoroutine(LobbyNetCom.UpdateClearSaveCo());
        ////--- Lobby
        ////--- InGame
        //else if (m_PacketBuff[0] == PacketType.BestScore)
        //    StartCoroutine(InGameNetCom.UpdateScoreCo());
        //else if (m_PacketBuff[0] == PacketType.UserGold)
        //    StartCoroutine(InGameNetCom.UpdateGoldCo());
        //else if (m_PacketBuff[0] == PacketType.InfoUpdate)
        //    StartCoroutine(InGameNetCom.UpdateInfoCo());
        //else if (m_PacketBuff[0] == PacketType.FloorUpdate)
        //    StartCoroutine(InGameNetCom.UpdateFloorCo());
        ////--- InGame
        ////--- Store
        //else if (m_PacketBuff[0] == PacketType.BuyRequest)
        //    StartCoroutine(StoreNetCom.BuyRequestCo());
        ////--- Store

        m_PacketBuff.RemoveAt(0);
    }

    public void PushPacket(PacketType a_PType)
    {
        bool a_isExist = false;
        for (int i = 0; i < m_PacketBuff.Count; i++)
        {
            if (m_PacketBuff[i] == a_PType) //���� ó�� ���� ���� ��Ŷ�� �����ϸ�
                a_isExist = true;
            //�� �߰����� �ʰ� ���� ������ ��Ŷ���� ������Ʈ �Ѵ�.
        }

        if (a_isExist == false)
            m_PacketBuff.Add(a_PType);
        //��� ���� �� Ÿ���� ��Ŷ�� ������ ���� �߰��Ѵ�.
    }

}
