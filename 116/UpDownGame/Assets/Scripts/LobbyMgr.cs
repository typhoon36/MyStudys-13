using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMgr : MonoBehaviour
{
    public Button m_Start_Btn;
    public Button m_Store_Btn;
    public Button m_Logout_Btn;
    public Button m_Clear_Save_Btn;
    float ClearLockTimer = 0.0f;     //데이터 초기화 대기 타이머

    public Text UserInfoText;

    [HideInInspector] public int m_MyRank = 0;
    public Button RestRk_Btn;  //Restore Ranking Button
    public Text   Ranking_Text;

    public Text MessageText;    //메시지 내용을 표시할 UI
    float  ShowMsTimer = 0.0f;  //메시지를 몇 초동안 보이게 할 건지에 대한 타이머

    [Header("--- ConfigBox ---")]
    public Button m_CfgBtn = null;
    public GameObject Canvas_Dialog = null;
    public GameObject m_ConfigBoxObj = null;    

    //--- 싱글턴 패턴
    public static LobbyMgr Inst = null;

    private void Awake()
    {
        Inst = this;
    }
    //--- 싱글턴 패턴

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f; //일시정지를 원래 속도로...
       //GlobalValue.LoadGameData();

        NetworkMgr.Inst.ReadyNetworkMgr(this);   //씬마다 추가

        if (m_Start_Btn != null)
            m_Start_Btn.onClick.AddListener(StartBtnClick);

        if (m_Store_Btn != null)
            m_Store_Btn.onClick.AddListener(() =>
            {
               // SceneManager.LoadScene("StoreScene");
            });

        if (m_Logout_Btn != null)
            m_Logout_Btn.onClick.AddListener(() =>
            {
                GlobalValue.ClearGameData();
                SceneManager.LoadScene("TitleScene");
            });

        if (m_Clear_Save_Btn != null)
            m_Clear_Save_Btn.onClick.AddListener(Clear_Save_Click);

        RefreshUserInfo();

#if AutoRestore
        //--- 자동 랭킹 갱신인 경우
        if(RestRk_Btn != null)
            RestRk_Btn.gameObject.SetActive(false);
        //--- 자동 랭킹 갱신인 경우
#else
        //--- 수동 랭킹 갱신인 경우
        if (RestRk_Btn != null)
            RestRk_Btn.onClick.AddListener(RestoreRank);
        //--- 수동 랭킹 갱신인 경우
#endif

        //--- 환경설정 Dlg 관련 구현 부분
        //if (m_CfgBtn != null)
        //    m_CfgBtn.onClick.AddListener(() =>
        //    {
        //        //닉네이 변경 요청 버튼
        //        if(m_ConfigBoxObj == null)
        //            m_ConfigBoxObj = Resources.Load("ConfigBox") as GameObject;

        //        GameObject a_CfgBoxObj = Instantiate(m_ConfigBoxObj);
        //        a_CfgBoxObj.transform.SetParent(Canvas_Dialog.transform, false);
        //        Time.timeScale = 0.0f;
        //    });
        //--- 환경설정 Dlg 관련 구현 부분
    }//void Start()

    // Update is called once per frame
    void Update()
    {
        if(0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if(ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false);    //메시지 끄기
            }
        }//if(0.0f < ShowMsTimer)

        if (0.0f < ClearLockTimer)
            ClearLockTimer -= Time.deltaTime;
    }

    void StartBtnClick()
    {
        //if(100 <= GlobalValue.g_CurFloorNum)
        //{
        //    //마지막 층에 도달한 상태에서 게임을 시작 했다면...
        //    //바로 직전 층(99층)에서 시작하게 하기...
        //   /// GlobalValue.g_CurFloorNum = 99;
        //    //PlayerPrefs.SetInt("CurFloorNum", GlobalValue.g_CurFloorNum);
        //}

        SceneManager.LoadScene("SampleScene");
    }

    void Clear_Save_Click()
    {
       // if(0.0f < ClearLockTimer)
       // {
       //     MessageOnOff("저장정보초기화 중입니다.");
       //     return;
       // }

       // PlayerPrefs.DeleteAll();
       //// GlobalValue.LoadGameData();
       // //RefreshUserInfo();

       // //NetworkMgr.Inst.LobbyNetCom.DltMethod = Result_Clear_Save;
       // NetworkMgr.Inst.PushPacket(PacketType.ClearSave);

       // ClearLockTimer = 5.0f;  //5초 동안 다시 시도 못하게 막는 걸로 하려는 의도
       // //서버로부터 응답을 받은 후에 다시 시도할 수 있게 허용하는 것으로 변경함
    }

   

    public void RefreshUserInfo()
    {
        UserInfoText.text = "내정보 : 별명(" + GlobalValue.g_NickName +
                            ") : 순위(" + m_MyRank + "등) : 점수(" +
                            GlobalValue.g_BestScore.ToString("N0") + "점) : 골드(" +
                            GlobalValue.g_UserGold.ToString("N0") + ")";
    }

    public void RefreshRankUI(RkRootInfo a_RkRootInfo)
    {
        Ranking_Text.text = "";

        for (int i = 0; i < a_RkRootInfo.RkList.Length; i++)
        {
            // 등수 안에 내가 있다면 색 표시
            if (a_RkRootInfo.RkList[i].user_id == GlobalValue.g_Unique_ID)
                Ranking_Text.text += "<color=#00ff00>";

            Ranking_Text.text += (i + 1).ToString() + "등 : " +
                                a_RkRootInfo.RkList[i].user_id +
                                " (" + a_RkRootInfo.RkList[i].nick_name + ") : " +
                                a_RkRootInfo.RkList[i].best_score + "점" + "\n";

            if (a_RkRootInfo.RkList[i].user_id == GlobalValue.g_Unique_ID)
                Ranking_Text.text += "</color>";
        }//for (int i = 0; i < a_RkRootInfo.RkList.Length; i++)

        m_MyRank = a_RkRootInfo.my_rank;

        RefreshUserInfo();
    }//public void RefreshRankUI(RkRootInfo a_RkRootInfo)

    public void MessageOnOff(string Mess = "", bool isOn = true, float a_Time = 5.0f)
    {
        if(isOn == true)
        {
            MessageText.text = Mess;
            MessageText.gameObject.SetActive(true);
            ShowMsTimer = a_Time;
        }
        else
        {
            MessageText.text = "";
            MessageText.gameObject.SetActive(false);
        }
    }

    void RestoreRank()
    {
        if (0.0f < NetworkMgr.Inst.LobbyNetCom.RestoreTimer)
        {
            MessageOnOff("최소 10초 주기로만 갱신됩니다.");
            return;
        }

        NetworkMgr.Inst.PushPacket(PacketType.GetRankingList);
        NetworkMgr.Inst.LobbyNetCom.RestoreTimer = 10.0f;
    }
}
