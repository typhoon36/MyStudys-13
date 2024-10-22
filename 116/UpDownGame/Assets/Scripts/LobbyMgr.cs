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
    float ClearLockTimer = 0.0f;     //������ �ʱ�ȭ ��� Ÿ�̸�

    public Text UserInfoText;

    [HideInInspector] public int m_MyRank = 0;
    public Button RestRk_Btn;  //Restore Ranking Button
    public Text   Ranking_Text;

    public Text MessageText;    //�޽��� ������ ǥ���� UI
    float  ShowMsTimer = 0.0f;  //�޽����� �� �ʵ��� ���̰� �� ������ ���� Ÿ�̸�

    [Header("--- ConfigBox ---")]
    public Button m_CfgBtn = null;
    public GameObject Canvas_Dialog = null;
    public GameObject m_ConfigBoxObj = null;    

    //--- �̱��� ����
    public static LobbyMgr Inst = null;

    private void Awake()
    {
        Inst = this;
    }
    //--- �̱��� ����

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f; //�Ͻ������� ���� �ӵ���...
       //GlobalValue.LoadGameData();

        NetworkMgr.Inst.ReadyNetworkMgr(this);   //������ �߰�

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
        //--- �ڵ� ��ŷ ������ ���
        if(RestRk_Btn != null)
            RestRk_Btn.gameObject.SetActive(false);
        //--- �ڵ� ��ŷ ������ ���
#else
        //--- ���� ��ŷ ������ ���
        if (RestRk_Btn != null)
            RestRk_Btn.onClick.AddListener(RestoreRank);
        //--- ���� ��ŷ ������ ���
#endif

        //--- ȯ�漳�� Dlg ���� ���� �κ�
        //if (m_CfgBtn != null)
        //    m_CfgBtn.onClick.AddListener(() =>
        //    {
        //        //�г��� ���� ��û ��ư
        //        if(m_ConfigBoxObj == null)
        //            m_ConfigBoxObj = Resources.Load("ConfigBox") as GameObject;

        //        GameObject a_CfgBoxObj = Instantiate(m_ConfigBoxObj);
        //        a_CfgBoxObj.transform.SetParent(Canvas_Dialog.transform, false);
        //        Time.timeScale = 0.0f;
        //    });
        //--- ȯ�漳�� Dlg ���� ���� �κ�
    }//void Start()

    // Update is called once per frame
    void Update()
    {
        if(0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if(ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false);    //�޽��� ����
            }
        }//if(0.0f < ShowMsTimer)

        if (0.0f < ClearLockTimer)
            ClearLockTimer -= Time.deltaTime;
    }

    void StartBtnClick()
    {
        //if(100 <= GlobalValue.g_CurFloorNum)
        //{
        //    //������ ���� ������ ���¿��� ������ ���� �ߴٸ�...
        //    //�ٷ� ���� ��(99��)���� �����ϰ� �ϱ�...
        //   /// GlobalValue.g_CurFloorNum = 99;
        //    //PlayerPrefs.SetInt("CurFloorNum", GlobalValue.g_CurFloorNum);
        //}

        SceneManager.LoadScene("SampleScene");
    }

    void Clear_Save_Click()
    {
       // if(0.0f < ClearLockTimer)
       // {
       //     MessageOnOff("���������ʱ�ȭ ���Դϴ�.");
       //     return;
       // }

       // PlayerPrefs.DeleteAll();
       //// GlobalValue.LoadGameData();
       // //RefreshUserInfo();

       // //NetworkMgr.Inst.LobbyNetCom.DltMethod = Result_Clear_Save;
       // NetworkMgr.Inst.PushPacket(PacketType.ClearSave);

       // ClearLockTimer = 5.0f;  //5�� ���� �ٽ� �õ� ���ϰ� ���� �ɷ� �Ϸ��� �ǵ�
       // //�����κ��� ������ ���� �Ŀ� �ٽ� �õ��� �� �ְ� ����ϴ� ������ ������
    }

   

    public void RefreshUserInfo()
    {
        UserInfoText.text = "������ : ����(" + GlobalValue.g_NickName +
                            ") : ����(" + m_MyRank + "��) : ����(" +
                            GlobalValue.g_BestScore.ToString("N0") + "��) : ���(" +
                            GlobalValue.g_UserGold.ToString("N0") + ")";
    }

    public void RefreshRankUI(RkRootInfo a_RkRootInfo)
    {
        Ranking_Text.text = "";

        for (int i = 0; i < a_RkRootInfo.RkList.Length; i++)
        {
            // ��� �ȿ� ���� �ִٸ� �� ǥ��
            if (a_RkRootInfo.RkList[i].user_id == GlobalValue.g_Unique_ID)
                Ranking_Text.text += "<color=#00ff00>";

            Ranking_Text.text += (i + 1).ToString() + "�� : " +
                                a_RkRootInfo.RkList[i].user_id +
                                " (" + a_RkRootInfo.RkList[i].nick_name + ") : " +
                                a_RkRootInfo.RkList[i].best_score + "��" + "\n";

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
            MessageOnOff("�ּ� 10�� �ֱ�θ� ���ŵ˴ϴ�.");
            return;
        }

        NetworkMgr.Inst.PushPacket(PacketType.GetRankingList);
        NetworkMgr.Inst.LobbyNetCom.RestoreTimer = 10.0f;
    }
}
