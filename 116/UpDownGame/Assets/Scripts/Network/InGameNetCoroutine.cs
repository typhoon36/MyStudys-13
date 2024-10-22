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
        if (GlobalValue.g_Unique_ID == "")  //정상적으로 로그인 되어 있지 않다면...
            yield break;        //코루틴 함수를 빠져 나가기...

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_score", GlobalValue.g_BestScore);

        //타임아웃 설정 (초 단위로 설정, 예: 3초)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        UnityWebRequest a_www = UnityWebRequest.Post(BestScoreUrl, form);
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

        if (a_www.error == null) //에러가 나지 않았을 때 동작
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

        //타임아웃 설정 (초 단위로 설정, 예: 3초)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        //요청 보내기
        a_www.SendWebRequest();

        //응답이 오거나 타임아웃이 발생할 때까지 대기
        while (!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null; //다음 프레임까지 대기
        }//while(!a_www.isDone && !isTimeOut)

        //타임아웃 처리
        if (isTimeOut)
        {
            a_www.Abort();  //요청을 중단
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;

            yield break;
        }

        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        string sz = enc.GetString(a_www.downloadHandler.data);

        bool a_isWait = false;
        string a_MsgStr = "";
        if (a_www.error == null)  //에러가 나지 않았을 때 동작
        {
            if (sz.Contains("Update Nick Success.") == true)
            {
                GlobalValue.g_NickName = a_NickStr;
               
            }
            else if (sz.Contains("Nickname does exist.") == true)
            {
                a_isWait = true;
                a_MsgStr = "중복된 닉네임이 존재합니다.";
            }
            else
            {
                a_isWait = true;
                a_MsgStr = sz;
            }
        }//if(a_www.error == null)  //에러가 나지 않았을 때 동작
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
