using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InGameNetCoroutine : MonoBehaviour
{
    string BestScoreUrl = "";
    string UpdateNickUrl = "";

    Game_Mgr m_RefGameMgr = null;

    void Start()
    {
        BestScoreUrl   = "http://typhoon.dothome.co.kr/NumberGame/UpdateBScore.php";
        UpdateNickUrl  = "http://typhoon.dothome.co.kr/NumberGame/UpdateNickname.php";
    }

    public void GameStart(Game_Mgr a_RefGMgr)
    {
        m_RefGameMgr = a_RefGMgr;
    }

    public IEnumerator UpdateScoreCo()
    {
        if (GlobalValue.g_Unique_ID == "")  //���������� �α��� �Ǿ� ���� �ʴٸ�...
            yield break;        //�ڷ�ƾ �Լ��� ���� ������...

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_score", GlobalValue.g_BestScore);

        //Ÿ�Ӿƿ� ���� (�� ������ ����, ��: 3��)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        UnityWebRequest a_www = UnityWebRequest.Post(BestScoreUrl, form);
        a_www.SendWebRequest();    //������ �ö����� ����ϱ�...

        //������ ���ų� Ÿ�Ӿƿ��� �߻��� ������ ���
        while (!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null; //���� �����ӱ��� ���
        }//while(!a_www.isDone && !isTimeOut)

        //Ÿ�Ӿƿ� ó��
        if (isTimeOut == true)
        {
            a_www.Abort();  //��û�� �ߴ�
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        if (a_www.error == null) //������ ���� �ʾ��� �� ����
        {
            //Debug.Log("UpdateSuccess~~");
        }
        else
        {
            Debug.Log(a_www.error);
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
    }

    public IEnumerator NickChangeCo(string a_NickStr)
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;

        if (a_NickStr == "")
            yield break;

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_nick", a_NickStr, System.Text.Encoding.UTF8);

        UnityWebRequest a_www = UnityWebRequest.Post(UpdateNickUrl, form);

        //Ÿ�Ӿƿ� ���� (�� ������ ����, ��: 3��)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        //��û ������
        a_www.SendWebRequest();

        //������ ���ų� Ÿ�Ӿƿ��� �߻��� ������ ���
        while (!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null; //���� �����ӱ��� ���
        }//while(!a_www.isDone && !isTimeOut)

        //Ÿ�Ӿƿ� ó��
        if (isTimeOut)
        {
            a_www.Abort();  //��û�� �ߴ�
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;

            yield break;
        }

        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        string sz = enc.GetString(a_www.downloadHandler.data);

        bool a_isWait = false;
        string a_MsgStr = "";
        if (a_www.error == null)  //������ ���� �ʾ��� �� ����
        {
            if (sz.Contains("Update Nick Success.") == true)
            {
                GlobalValue.g_NickName = a_NickStr;
               
            }
            else if (sz.Contains("Nickname does exist.") == true)
            {
                a_isWait = true;
                a_MsgStr = "�ߺ��� �г����� �����մϴ�.";
            }
            else
            {
                a_isWait = true;
                a_MsgStr = sz;
            }
        }//if(a_www.error == null)  //������ ���� �ʾ��� �� ����
        else
        {
            a_isWait = true;
            a_MsgStr = sz + " : " + a_www.error;
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;


        if(m_RefGameMgr!= null && a_isWait == true)
        {
            m_RefGameMgr.MsgOnOff(a_MsgStr);
        }

    }



}//public class InGameNetCoroutine : MonoBehaviour
