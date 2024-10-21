using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game_Mgr : MonoBehaviour
{
    public Button Equal_Btn;        //��ġ�Ѵ� ��ư
    public Button Small_Btn;        //~���� �۴� ��ư
    public Button Big_Btn;          //~���� ũ�� ��ư
    public Button Replay_Btn;       //���� �ٽ��ϱ� ��ư

    public Text UserInfo_Text;
    public Text ComQuestion_Text;
    public Text Result_Text;

    int m_Count  = 0;       //���� Ƚ��
    int m_CurNum = 0;       //���������� ���� �������� ���� ���� (���� ��)
    int m_Min    = 1;       //�ּҰ�
    int m_Max    = 100;     //�ִ밪
    bool m_IsGameOver = false;  //���� ���� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        m_CurNum = Random.Range(m_Min, (m_Max + 1));  // 1 ~ 100 ������ �߻�
        ComQuestion_Text.text = "����� ������ ���ڴ� " + m_CurNum + "�Դϱ�?";

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

        Result_Text.text = "����� ������ ���ڴ� " + m_CurNum + "�Դϴ�.";
        UserInfo_Text.text = "���� Ƚ�� : 20�� �� " + m_Count + "��";

        m_IsGameOver = true;
    }

    private void SmallBtnClick()
    {
        if(m_IsGameOver == true)
            return; 

        m_Max = m_CurNum - 1;
        m_Count++;

        if(m_Max < m_Min) //������ �� �� ������ ���, ����ó��
        {
            Result_Text.text = "��ư�� �� �� �����ϼ̽��ϴ�.(�ٽ� ����)";
        }
        else if(m_Min == m_Max) //��ġ�ϴ� ���
        {
            Result_Text.text = "����� ������ ���ڴ� " + m_Max + "�Դϴ�.";
            UserInfo_Text.text = "���� Ƚ�� : 20�� �� " + m_Count + "��";

            m_IsGameOver = true;
            return;
        }
        else //���������� max�� min���� ū ���
        {
            if(20 <= m_Count)
            {
                Result_Text.text = "����~~";
                UserInfo_Text.text = "���� Ƚ�� : 20�� �� " + m_Count + "��";

                m_IsGameOver = true;
                return;
            }

            m_CurNum = Random.Range(m_Min, (m_Max + 1));
            ComQuestion_Text.text = "����� ������ ���ڴ� " + m_CurNum + "�Դϱ�?";
            UserInfo_Text.text = "���� Ƚ�� : 20�� �� " + m_Count + "��";
        }////���������� max�� min���� ū ���

    }//private void SmallBtnClick()

    private void BigBtnClick()
    {
        if(m_IsGameOver == true)
            return;

        m_Min = m_CurNum + 1;
        m_Count++;

        if(m_Max < m_Min)
        {
            Result_Text.text = "��ư�� �� �� �����ϼ̽��ϴ�.(�ٽ� ����)";
        }
        else if(m_Min == m_Max) //��ġ�ϴ� ���
        {
            Result_Text.text = "����� ������ ���ڴ� " + m_Min + "�Դϴ�.";
            UserInfo_Text.text = "���� Ƚ�� : 20�� �� " + m_Count + "��";

            m_IsGameOver = true;
            return;
        }
        else  //���������� max�� min���� ū ���
        {
            if (20 <= m_Count)
            {
                Result_Text.text = "����~~";
                UserInfo_Text.text = "���� Ƚ�� : 20�� �� " + m_Count + "��";

                m_IsGameOver = true;
                return;
            }

            m_CurNum = Random.Range(m_Min, (m_Max + 1));
            ComQuestion_Text.text = "����� ������ ���ڴ� " + m_CurNum + "�Դϱ�?";
            UserInfo_Text.text = "���� Ƚ�� : 20�� �� " + m_Count + "��";
        }

    }//private void BigBtnClick()

    private void ReplayBtnClick()
    {
        SceneManager.LoadScene("SampleScene");
    }

}//public class Game_Mgr : MonoBehaviour
