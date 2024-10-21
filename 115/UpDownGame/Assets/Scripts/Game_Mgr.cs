using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game_Mgr : MonoBehaviour
{
    public Button Equal_Btn;        //일치한다 버튼
    public Button Small_Btn;        //~보다 작다 버튼
    public Button Big_Btn;          //~보다 크다 버튼
    public Button Replay_Btn;       //게임 다시하기 버튼

    public Text UserInfo_Text;
    public Text ComQuestion_Text;
    public Text Result_Text;

    int m_Count  = 0;       //진행 횟수
    int m_CurNum = 0;       //마지막으로 나온 랜덤값을 위한 변수 (질문 값)
    int m_Min    = 1;       //최소값
    int m_Max    = 100;     //최대값
    bool m_IsGameOver = false;  //게임 종료 여부 변수

    // Start is called before the first frame update
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void EqualBtnClick()
    {
        if (m_IsGameOver == true)
            return;

        Result_Text.text = "당신이 생각한 숫자는 " + m_CurNum + "입니다.";
        UserInfo_Text.text = "진행 횟수 : 20번 중 " + m_Count + "번";

        m_IsGameOver = true;
    }

    private void SmallBtnClick()
    {
        if(m_IsGameOver == true)
            return; 

        m_Max = m_CurNum - 1;
        m_Count++;

        if(m_Max < m_Min) //유저가 잘 못 생각한 경우, 예외처리
        {
            Result_Text.text = "버튼을 잘 못 선택하셨습니다.(다시 진행)";
        }
        else if(m_Min == m_Max) //일치하는 경우
        {
            Result_Text.text = "당신이 생각한 숫자는 " + m_Max + "입니다.";
            UserInfo_Text.text = "진행 횟수 : 20번 중 " + m_Count + "번";

            m_IsGameOver = true;
            return;
        }
        else //아직까지는 max가 min보다 큰 경우
        {
            if(20 <= m_Count)
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

    }//private void SmallBtnClick()

    private void BigBtnClick()
    {
        if(m_IsGameOver == true)
            return;

        m_Min = m_CurNum + 1;
        m_Count++;

        if(m_Max < m_Min)
        {
            Result_Text.text = "버튼을 잘 못 선택하셨습니다.(다시 진행)";
        }
        else if(m_Min == m_Max) //일치하는 경우
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

    }//private void BigBtnClick()

    private void ReplayBtnClick()
    {
        SceneManager.LoadScene("SampleScene");
    }

}//public class Game_Mgr : MonoBehaviour
