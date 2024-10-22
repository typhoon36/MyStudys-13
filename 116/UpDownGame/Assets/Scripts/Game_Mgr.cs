using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game_Mgr : MonoBehaviour
{
    public Button Equal_Btn;        //��ġ�Ѵ� ��ư
    public Button Small_Btn;        //~���� �۴� ��ư
    public Button Big_Btn;          //~���� ũ�� ��ư
    public Button Replay_Btn;       //���� �ٽ��ϱ� ��ư
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

    int m_Count = 0;       //���� Ƚ��
    int m_CurNum = 0;       //���������� ���� �������� ���� ���� (���� ��)
    int m_Min = 1;       //�ּҰ�
    int m_Max = 100;     //�ִ밪
    bool m_IsGameOver = false;  //���� ���� ���� ����

    // Start is called before the first frame update
    void Start()
    {

        NetworkMgr.Inst.ReadyNetworkMgr(this);

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
            BestScore_Txt.text = "�ְ��� : " + GlobalValue.g_BestScore;

    }

    private void EqualBtnClick()
    {
        if (m_IsGameOver == true)
            return;

        Result_Text.text = "����� ������ ���ڴ� " + m_CurNum + "�Դϴ�.";
        UserInfo_Text.text = "���� Ƚ�� : 20�� �� " + m_Count + "��";

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

        if (m_Max < m_Min) //������ �� �� ������ ���, ����ó��
        {
            Result_Text.text = "��ư�� �� �� �����ϼ̽��ϴ�.(�ٽ� ����)";
        }
        else if (m_Min == m_Max) //��ġ�ϴ� ���
        {
            Result_Text.text = "����� ������ ���ڴ� " + m_Max + "�Դϴ�.";
            UserInfo_Text.text = "���� Ƚ�� : 20�� �� " + m_Count + "��";

            m_IsGameOver = true;
            return;
        }
        else //���������� max�� min���� ū ���
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
        }////���������� max�� min���� ū ���


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
            Result_Text.text = "��ư�� �� �� �����ϼ̽��ϴ�.(�ٽ� ����)";
        }
        else if (m_Min == m_Max) //��ġ�ϴ� ���
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
            MsgOnOff("��Ȯ�� �Է��� �ּ���." , true);
            return;
        }

        NetworkMgr.Inst.m_NickCgBuff = a_NickStr;

        NetworkMgr.Inst.PushPacket(PacketType.NickUpdate);

    }

}//public class Game_Mgr : MonoBehaviour
