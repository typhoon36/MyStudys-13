using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game_Mgr : MonoBehaviour
{
    public Button Equal_Btn;        //일치한다 버튼
    public Button Small_Btn;        //~보다 작다 버튼
    public Button Big_Btn;          //~보다 크다 버튼
    public Button Replay_Btn;       //게임 다시하기 버튼
    public Button Back_Btn;

    public Text UserInfo_Text;
    public Text ComQuestion_Text;
    public Text Result_Text;
    public Text BestScore_Txt;

    [Header("Message")]
    public Text Message_Txt;
    float ShowMsTime = 0.0f;

    public InputField NickIF;
    public Button m_NickChangeBtn;

    int m_Count = 0;       //진행 횟수
    int m_CurNum = 0;       //마지막으로 나온 랜덤값을 위한 변수 (질문 값)
    int m_Min = 1;       //최소값
    int m_Max = 100;     //최대값
    bool m_IsGameOver = false;  //게임 종료 여부 변수

    // Start is called before the first frame update
    void Start()
    {

        NetworkMgr.Inst.ReadyNetworkMgr(this);

        m_CurNum = Random.Range(m_Min, (m_Max + 1));  // 1 ~ 100 랜덤값 발생
        ComQuestion_Text.text = "당신이 생각한 숫자는 " + m_CurNum + "입니까?";

        if (Equal_Btn != null)
            Equal_Btn.onClick.AddListener(EqualBtnClick);

        if (Small_Btn != null)
            Small_Btn.onClick.AddListener(SmallBtnClick);

        if (Big_Btn != null)
            Big_Btn.onClick.AddListener(BigBtnClick);

        if (Replay_Btn != null)
            Replay_Btn.onClick.AddListener(ReplayBtnClick);

        if (Back_Btn != null)
            Back_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Lobby");
            });

        NickIF.text = GlobalValue.g_NickName;

        if (m_NickChangeBtn != null)
            m_NickChangeBtn.onClick.AddListener(NickChangeCo);


    }
    private void Update()
    {
        if(0 < ShowMsTime)
        {
            ShowMsTime -= Time.deltaTime;

            if(ShowMsTime <= 0.0f)
            {
                MsgOnOff("", false);
            }
            
        }

        if (BestScore_Txt != null)
            BestScore_Txt.text = "최고기록 : " + GlobalValue.g_BestScore;

    }

    private void EqualBtnClick()
    {
        if (m_IsGameOver == true)
            return;

        Result_Text.text = "당신이 생각한 숫자는 " + m_CurNum + "입니다.";
        UserInfo_Text.text = "진행 횟수 : 20번 중 " + m_Count + "번";

        m_IsGameOver = true;

        if (GlobalValue.g_BestScore < int.MaxValue - 1)
        {
            GlobalValue.g_BestScore++;
            NetworkMgr.Inst.PushPacket(PacketType.BestScore);
        }


    }

    private void SmallBtnClick()
    {
        if (m_IsGameOver == true)
            return;

        m_Max = m_CurNum - 1;
        m_Count++;

        if (m_Max < m_Min) //유저가 잘 못 생각한 경우, 예외처리
        {
            Result_Text.text = "버튼을 잘 못 선택하셨습니다.(다시 진행)";
        }
        else if (m_Min == m_Max) //일치하는 경우
        {
            Result_Text.text = "당신이 생각한 숫자는 " + m_Max + "입니다.";
            UserInfo_Text.text = "진행 횟수 : 20번 중 " + m_Count + "번";

            m_IsGameOver = true;
            return;
        }
        else //아직까지는 max가 min보다 큰 경우
        {
            if (20 <= m_Count)
            {
                Result_Text.text = "실패~~";
                UserInfo_Text.text = "진행 횟수 : 20번 중 " + m_Count + "번";

                m_IsGameOver = true;
                return;
            }

            m_CurNum = Random.Range(m_Min, (m_Max + 1));
            ComQuestion_Text.text = "당신이 생각한 숫자는 " + m_CurNum + "입니까?";
            UserInfo_Text.text = "진행 횟수 : 20번 중 " + m_Count + "번";
        }////아직까지는 max가 min보다 큰 경우


        if (GlobalValue.g_BestScore < int.MaxValue - 1)
        {
            GlobalValue.g_BestScore++;
            NetworkMgr.Inst.PushPacket(PacketType.BestScore);
        }

    }//private void SmallBtnClick()

    private void BigBtnClick()
    {
        if (m_IsGameOver == true)
            return;

        m_Min = m_CurNum + 1;
        m_Count++;

        if (m_Max < m_Min)
        {
            Result_Text.text = "버튼을 잘 못 선택하셨습니다.(다시 진행)";
        }
        else if (m_Min == m_Max) //일치하는 경우
        {
            Result_Text.text = "당신이 생각한 숫자는 " + m_Min + "입니다.";
            UserInfo_Text.text = "진행 횟수 : 20번 중 " + m_Count + "번";

            m_IsGameOver = true;
            return;
        }
        else  //아직까지는 max가 min보다 큰 경우
        {
            if (20 <= m_Count)
            {
                Result_Text.text = "실패~~";
                UserInfo_Text.text = "진행 횟수 : 20번 중 " + m_Count + "번";

                m_IsGameOver = true;
                return;
            }

            m_CurNum = Random.Range(m_Min, (m_Max + 1));
            ComQuestion_Text.text = "당신이 생각한 숫자는 " + m_CurNum + "입니까?";
            UserInfo_Text.text = "진행 횟수 : 20번 중 " + m_Count + "번";
        }

        if (GlobalValue.g_BestScore < int.MaxValue - 1)
        {
            GlobalValue.g_BestScore++;
            NetworkMgr.Inst.PushPacket(PacketType.BestScore);
        }

    }//private void BigBtnClick()

    private void ReplayBtnClick()
    {
        SceneManager.LoadScene("SampleScene");
    }


    public void MsgOnOff(string Msg = "", bool IsShow = true)
    {
        if(IsShow == true)
        {
            Message_Txt.text = Msg;
            Message_Txt.gameObject.SetActive(true);
            Message_Txt.color = Color.yellow;
            ShowMsTime = 5.0f;
        }
        else
        {
            Message_Txt.text = "";
            Message_Txt.gameObject.SetActive(false);
            ShowMsTime = 0.0f;
        }
    }

    void NickChangeCo()
    {
        string a_NickStr = NickIF.text;
        a_NickStr = a_NickStr.Trim();

        if (a_NickStr == "" || a_NickStr.Length < 3)
        {
            MsgOnOff("정확히 입력해 주세요." , true);
            return;
        }

        NetworkMgr.Inst.m_NickCgBuff = a_NickStr;

        NetworkMgr.Inst.PushPacket(PacketType.NickUpdate);

    }

}//public class Game_Mgr : MonoBehaviour
