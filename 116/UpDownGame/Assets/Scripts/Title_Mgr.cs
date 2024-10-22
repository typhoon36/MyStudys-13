using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title_Mgr : MonoBehaviour
{
    [Header("--- LoginPanel ---")]
    public GameObject m_LoginPanelObj;
    public Button m_LoginBtn = null;
    public Button m_CreateAccOpenBtn = null;
    public InputField IdInputField;
    public InputField PwInputField;

    [Header("--- CreateAccountPanel ---")]
    public GameObject m_CreatePanelObj;
    public InputField New_IdInputField;
    public InputField New_PwInputField;
    public InputField New_NickInputField;
    public Button m_CreateAccountBtn = null;
    public Button m_CancelBtn = null;

    [Header("--- Message ---")]
    public Text MessageText;
    float ShowMsTimer = 0.0f;

    void Start()
    {
        NetworkMgr.Inst.ReadyNetworkMgr(this);  //씬마다 추가

        //--- LoginPanel
        if (m_LoginBtn != null)
            m_LoginBtn.onClick.AddListener(LoginBtnClick);

        if (m_CreateAccOpenBtn != null)
            m_CreateAccOpenBtn.onClick.AddListener(OpenCreateAccBtn);

        //--- CreateAccountPanel
        if (m_CancelBtn != null)
            m_CancelBtn.onClick.AddListener(CancelBtnClick);

        if (m_CreateAccountBtn != null)
            m_CreateAccountBtn.onClick.AddListener(CreateAccountBtn);
    }

    void Update()
    {
        if (0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if (ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false);
            }
        }
    }
    void LoginBtnClick()
    {
        //SceneManager.LoadScene("Lobby");

        string a_IdStr = IdInputField.text;
        string a_PwStr = PwInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();

        if (string.IsNullOrEmpty(a_IdStr) == true ||
           string.IsNullOrEmpty(a_PwStr) == true)
        {
            MessageOnOff("Id, Pw는 빈칸 없이 입력해 주세요.");
            return;
        }

        if (!(3 <= a_IdStr.Length && a_IdStr.Length <= 20))
        {
            MessageOnOff("Id는 3글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        if (!(4 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            MessageOnOff("Pw는 4글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        NetworkMgr.Inst.m_IdStrBuff = a_IdStr;
        NetworkMgr.Inst.m_PwStrBuff = a_PwStr;
        NetworkMgr.Inst.PushPacket(PacketType.Login);
    }

    void OpenCreateAccBtn()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(false);

        if (m_CreatePanelObj != null)
            m_CreatePanelObj.SetActive(true);
    }

    void CancelBtnClick()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(true);

        if (m_CreatePanelObj != null)
            m_CreatePanelObj.SetActive(false);
    }

    void CreateAccountBtn()
    {
        string a_IdStr = New_IdInputField.text;
        string a_PwStr = New_PwInputField.text;
        string a_NickStr = New_NickInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();
        a_NickStr = a_NickStr.Trim();

        if (string.IsNullOrEmpty(a_IdStr) == true ||
           string.IsNullOrEmpty(a_PwStr) == true ||
           string.IsNullOrEmpty(a_NickStr) == true)
        {
            MessageOnOff("Id, Pw, 별명은 빈칸 없이 입력해 주세요.");
            return;
        }

        if (!(3 <= a_IdStr.Length && a_IdStr.Length <= 20))
        {
            MessageOnOff("Id는 3글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        if (!(4 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            MessageOnOff("Pw는 4글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        if (!(2 <= a_NickStr.Length && a_NickStr.Length <= 20))
        {
            MessageOnOff("별명는 2글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        NetworkMgr.Inst.m_IdStrBuff = a_IdStr;
        NetworkMgr.Inst.m_PwStrBuff = a_PwStr;
        NetworkMgr.Inst.m_NickStrBuff = a_NickStr;
        NetworkMgr.Inst.PushPacket(PacketType.CreateAccount);
    }

    public void MessageOnOff(string Msg = "", bool isOn = true)
    {
        if (isOn == true)
        {
            MessageText.text = Msg;
            MessageText.gameObject.SetActive(true);
            ShowMsTimer = 7.0f;
        }
        else
        {
            MessageText.text = "";
            MessageText.gameObject.SetActive(false);
        }
    }

    public void LobbyLoadScene()
    {
        SceneManager.LoadScene("Lobby");
    }
}
