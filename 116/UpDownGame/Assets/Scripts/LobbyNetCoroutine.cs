using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class UserInfo
{
    public string user_id;
    public string nick_name;
    public int best_score;
}

[System.Serializable]
public class RkRootInfo
{
    public UserInfo[] RkList;
    public int my_rank;
}

public class LobbyNetCoroutine : MonoBehaviour
{
    string GetRankListUrl = "";
    //string UpdateNickUrl = "";
    //string ClearDataUrl = "";

    //랭킹 갱신 딜레이 타임
    [HideInInspector] public float RestoreTimer = 0.0f;
    RkRootInfo m_RkList = new RkRootInfo();

    LobbyMgr m_RefLobbyMgr = null;

    // Start is called before the first frame update
    void Start()
    {
        GetRankListUrl = "http://typhoon.dothome.co.kr/NumberGame/Get_ID_Rank.php";
        //UpdateNickUrl  = "http://xxxxxx.dothome.co.kr/xxxxxx/UpdateNickname.php";
        //ClearDataUrl   = "http://xxxxxx.dothome.co.kr/xxxxxx/ClearData.php";
    }

    public void LobbyStart(LobbyMgr a_RefLMgr)
    {
        RestoreTimer = 3.0f;   //랭킹 갱신 타이머
        NetworkMgr.Inst.PushPacket(PacketType.GetRankingList);
        m_RefLobbyMgr = a_RefLMgr;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_RefLobbyMgr == null)
            return;

#if AutoRestore
        //--- 자동 랭킹 갱신인 경우
        RestoreTimer -= Time.deltaTime;
        if (RestoreTimer <= 0.0f)
        {
            NetworkMgr.Inst.PushPacket(PacketType.GetRankingList);
            RestoreTimer = 10.0f;  //주기
        }
        //--- 자동 랭킹 갱신인 경우
#else
        //--- 수동 랭킹 갱신의 경우
        if (0.0f < RestoreTimer)
            RestoreTimer -= Time.deltaTime;
        //--- 수동 랭킹 갱신의 경우
#endif
    }

    public IEnumerator GetRankListCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;    //로그인 실패 상태라면 그냥 리턴

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);

        //타임아웃 설정 (초 단위로 설정, 예: 3초)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        UnityWebRequest a_www = UnityWebRequest.Post(GetRankListUrl, form);

        a_www.SendWebRequest();    //응답이 올때까지 대기하기...

        //응답이 오거나 타임아웃이 발생할 때까지 대기
        while (!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null; //다음 프레임까지 대기
        }//while(!a_www.isDone && !isTimeOut)

        //타임아웃 처리
        if (isTimeOut == true)
        {
            a_www.Abort();  //요청을 중단
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        if (a_www.error == null)  //에러가 나지 않았다면...
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string a_ReStr = enc.GetString(a_www.downloadHandler.data);

            //Debug.Log(a_ReStr);

            if (a_ReStr.Contains("Get_Rank_List_Success~") == true)
            {
                a_ReStr = a_ReStr.Replace("\nGet_Rank_List_Success~", "");

                RecRankList_MyRank(a_ReStr); //점수를 표시하는 함수를 호출
            }
            else
            {
                if(m_RefLobbyMgr != null)
                   m_RefLobbyMgr.MessageOnOff("순위 불러오기 실패 잠시 후 다시 시도해 주세요.");
            }
        }
        else
        {
            if (m_RefLobbyMgr != null)
            {
                m_RefLobbyMgr.MessageOnOff("순위 불러오기 실패 잠시 후 다시 시도해 주세요.");
                Debug.Log(a_www.error);
            }
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;

    }//IEnumerator GetRankListCo()

    void RecRankList_MyRank(string strJson)
    {
        if (strJson.Contains("RkList") == false)
            return;

        //Json 파일 파싱
        RkRootInfo a_RkList = JsonUtility.FromJson<RkRootInfo>(strJson);

        if (a_RkList == null)
            return;

        if(m_RefLobbyMgr != null)
           m_RefLobbyMgr.RefreshRankUI(a_RkList);
    }

   

    //public IEnumerator UpdateClearSaveCo() //웹서버에 현재 유저의 모든 정보 초기화 요청 함수
    //{
    //    if (GlobalValue.g_Unique_ID == "")
    //        yield break;

    //    WWWForm form = new WWWForm();
    //    form.AddField("Input_user", GlobalValue.g_Unique_ID,
    //                                System.Text.Encoding.UTF8);

    //    UnityWebRequest a_www = UnityWebRequest.Post(ClearDataUrl, form);

    //    bool isTimeOut = false;
    //    float startTime = Time.unscaledTime;

    //    NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

    //    a_www.SendWebRequest();    //응답이 올 때까지 대기하기...

    //    //응답이 오거나 타임아웃이 발생할 때까지 대기
    //    while (!a_www.isDone && !isTimeOut)
    //    {
    //        if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
    //        {
    //            isTimeOut = true;
    //        }

    //        yield return null; //다음 프레임까지 대기
    //    }//while(!a_www.isDone && !isTimeOut)

    //    //타임아웃 처리
    //    if (isTimeOut == true)
    //    {
    //        a_www.Abort();  //요청을 중단
    //        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
    //        yield break;
    //    }//if(isTimeOut == true)

    //    if (a_www.error == null)  //에러가 나지 않았을 때 동작
    //    {
    //        Debug.Log("Clear Data Success");

    //        //--- ClearGameData
    //        GlobalValue.g_BestScore = 0;    //게임점수
    //        GlobalValue.g_UserGold = 0;    //게임머니
    //        GlobalValue.g_Exp = 0;          //경험치 Experience
    //        GlobalValue.g_Level = 0;        //레벨

    //        GlobalValue.g_BestFloor = 1;    //최종 도달 건물 층수
    //        GlobalValue.g_CurFloorNum = 1;  //현재 건물 층수

    //        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
    //            GlobalValue.g_SkillCount[i] = 1;    //내 아이템 보유 정보
    //        //--- ClearGameData

    //        if (m_RefLobbyMgr != null)
    //            m_RefLobbyMgr.RefreshUserInfo();  //점수, 골드값 UI 초기화
    //    }
    //    else
    //    {
    //        Debug.Log(a_www.error);
    //    }

    //    //랭킹 다시 불러오기...
    //    NetworkMgr.Inst.PushPacket(PacketType.GetRankingList);
    //    RestoreTimer = 10.0f;
    //    //랭킹 다시 불러오기...

    //    a_www.Dispose();
    //    NetworkMgr.Inst.m_NetWaitTimer = 0.0f;

    //}//void UpdateClearSaveCo() 

}
