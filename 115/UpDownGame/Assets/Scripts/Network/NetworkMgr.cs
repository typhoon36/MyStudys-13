using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum PacketType
{
    //--- Title
    Login,          //로그인 요청
    CreateAccount,  //계정 생성 요청
    //--- Title

    //--- Lobby
    GetRankingList, //랭킹 받아오기
    NickUpdate,     //닉네임갱신
    ClearSave,      //서버에 저장된 내용 초기화 하기 
    //--- Lobby

    //--- InGame
    BestScore,      //최고점수
    UserGold,       //유저골드
    InfoUpdate,     //각종정보갱신, 아이템정보
    FloorUpdate,    //층정보
    //--- InGame

    //--- Store
    BuyRequest,     //상품 구매 요청
    //--- Store
}

[System.Serializable]
public class ItemList
{
    public int[] SkList;
}

[System.Serializable]
public class FloorInfo   //층 정보 로딩 
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
    public string floor_info;  //이 필드는 문자열로 유지하고, 추가적으로 파싱이 필요함
    public string info;        //이 필드는 문자열로 유지하고, 추가적으로 파싱이 필요함
}

public class NetworkMgr : G_Singleton<NetworkMgr>
{
    //--- 서버에 전송할 패킷 처리용 큐 관련 변수
    //[HideInInspector] public bool isNetworkLock = false; //Network 대기 상태 여부 변수
    [HideInInspector] public float m_NetWaitTimer = 0.0f;  //Network 대기 상태 여부 변수
    List<PacketType> m_PacketBuff = new List<PacketType>();
    //--- 서버에 전송할 패킷 처리용 큐 관련 변수

    //3초 동안 응답이 없으면 다음 패킷 처리 가능하도록...
    public const float m_Timeout = 3.0f;

    //--- Scene별 컴포넌트 객체
    [HideInInspector] public TitleNetCoroutine TitleNetCom;
    //[HideInInspector] public LobbyNetCoroutine LobbyNetCom;
    //[HideInInspector] public InGameNetCoroutine InGameNetCom;
    //[HideInInspector] public StoreNetCoroutine StoreNetCom;
    //--- Scene별 컴포넌트 객체

    //--- 로그인, 계정생성 매개변수로 넘겨 받을 임시변수
    [HideInInspector] public string m_IdStrBuff = "";
    [HideInInspector] public string m_PwStrBuff = "";
    [HideInInspector] public string m_NickStrBuff = "";
    //--- 로그인, 계정생성 매개변수로 넘겨 받을 임시변수

    //--- 닉네임 변경에 필요한 변수들
    [HideInInspector] public string m_NickCgBuff = "";
    //[HideInInspector] public ConfigBox m_RefCfgBox = null;
    //--- 닉네임 변경에 필요한 변수들

    protected override void Init()  //Awake() 함수 대신 사용
    {
        base.Init(); //부모쪽에 있는 Init() 함수 호출

        TitleNetCom  = gameObject.AddComponent<TitleNetCoroutine>();   //컴포넌트 추가
        //LobbyNetCom  = gameObject.AddComponent<LobbyNetCoroutine>();   //컴포넌트 추가
        //InGameNetCom = gameObject.AddComponent<InGameNetCoroutine>();  //컴포넌트 추가
        //StoreNetCom  = gameObject.AddComponent<StoreNetCoroutine>();
    }

    public void ReadyNetworkMgr(MonoBehaviour a_CurMgr)  //초기화시 한번 호출
    {
        //as 연산자는 주어진 객체가 지정한 타입으로 캐스팅이 가능한지 확인하고,
        //가능하면 해당 타입으로 변환하며, 불가능하면 null을 반환합니다.
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

        //if (isNetworkLock == false)  //지금 패킷 처리 중인 상태가 아니면...
        if(m_NetWaitTimer <= 0.0f) //지금 패킷 처리 중인 상태가 아니면...
        {
            if (0 < m_PacketBuff.Count)  //대기 패킷이 존재한다면...
            {
                Req_Network();
            }
        }//if(m_NetWaitTimer <= 0.0f) //지금 패킷 처리 중인 상태가 아니면...
    }

    void Req_Network()  //RequestNetwork
    {
        //--- Title
        if (m_PacketBuff[0] == PacketType.Login)   //로그인 요청
            StartCoroutine(TitleNetCom.LoginCo(m_IdStrBuff, m_PwStrBuff));
        else if (m_PacketBuff[0] == PacketType.CreateAccount)  //계정 생성 요청
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
            if (m_PacketBuff[i] == a_PType) //아직 처리 되지 않은 패킷이 존재하면
                a_isExist = true;
            //또 추가하지 않고 기존 버퍼의 패킷으로 업데이트 한다.
        }

        if (a_isExist == false)
            m_PacketBuff.Add(a_PType);
        //대기 중인 이 타입의 패킷이 없으면 새로 추가한다.
    }

}
