using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TitleNetCoroutine : MonoBehaviour
{
    string LoginUrl = "";
    string CreateUrl = "";

    Title_Mgr m_RefTitleMgr = null;


    void Start()
    {
        LoginUrl = "http://typhoon.dothome.co.kr/NumberGame/Login.php";
        CreateUrl = "http://typhoon.dothome.co.kr/NumberGame/CreateAccount.php";
    }

    public void TitleStart(Title_Mgr a_RefTMgr)
    {
        m_RefTitleMgr = a_RefTMgr;
    }

    public IEnumerator LoginCo(string a_IdStr, string a_PwStr)
    {
        WWWForm form = new WWWForm();

        form.AddField("Input_id", a_IdStr, System.Text.Encoding.UTF8);
        form.AddField("Input_pw", a_PwStr);

        UnityWebRequest a_www = UnityWebRequest.Post(LoginUrl, form);

        //타임아웃 설정 (초 단위로 설정, 예: 3초)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        yield return a_www.SendWebRequest();    //서버로부터 응답이 올 때까지 대기하기...

        //응답이 오거나 타임아웃이 발생할 때까지 대기
        while (!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null; 
        }


        if (isTimeOut == true)
        {
            a_www.Abort();  
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        bool IsExit = false;
        if (a_www.error == null) 
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);

            if (sz.Contains("Id does not exist.") == true)
            {
                if(m_RefTitleMgr != null)
                    m_RefTitleMgr.MessageOnOff("아이디가 존재하지 않습니다.");
                IsExit = true;
            }

            else if (sz.Contains("Password does not Match.") == true)
            {
                if (m_RefTitleMgr != null)
                    m_RefTitleMgr.MessageOnOff("비밀번호가 일치하지 않습니다.");
                IsExit = true;
            }

            else if (sz.Contains("Login_Success!!") == false)
            {
                if (m_RefTitleMgr != null)
                    m_RefTitleMgr.MessageOnOff("로그인 실패, 잠시 후 다시 시도하거나 운영진에 문의해 주세요." + sz);
                IsExit = true;
            }

            else if (sz.Contains("{\"") == false) //JSON 형식이 맞는지 확인 하는 코드
            {
                if (m_RefTitleMgr != null)
                    m_RefTitleMgr.MessageOnOff("서버의 응답이 정상적이지 않습니다." + sz);
                IsExit = true;
            }

            if(IsExit == true)
            {
                a_www.Dispose();
                NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
                yield break;  //코루틴 함수를 즉시 빠져나가는 명령어
            }

            GlobalValue.g_Unique_ID = a_IdStr; //유저의 고유번호

            string a_GetStr = sz.Substring(sz.IndexOf("{\""));
            a_GetStr = a_GetStr.Replace("\nLogin_Success!!", "");

            SvRespon response = JsonUtility.FromJson<SvRespon>(a_GetStr); //Json 파싱

            GlobalValue.g_NickName = response.nick_name;
            GlobalValue.g_BestScore = response.best_score;
            GlobalValue.g_UserGold = response.game_gold;

            Debug.Log(sz);

            if (m_RefTitleMgr != null)
                m_RefTitleMgr.LobbyLoadScene();
        }
        else
        {
            if (m_RefTitleMgr != null)
                m_RefTitleMgr.MessageOnOff(a_www.error);
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
    }

    public IEnumerator CreateActCo(string a_IdStr, string a_PwStr, string a_NickStr)
    {
        WWWForm form = new WWWForm();

        form.AddField("Input_id", a_IdStr, System.Text.Encoding.UTF8);
        form.AddField("Input_pw", a_PwStr);
        form.AddField("Input_nick", a_NickStr, System.Text.Encoding.UTF8);

        UnityWebRequest a_www = UnityWebRequest.Post(CreateUrl, form);

        //타임아웃 설정 (초 단위로 설정, 예: 3초)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        a_www.SendWebRequest(); //응답이 올 때까지 대기하기...

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

        if (a_www.error == null)  //에러가 없을 때 
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);

            if (m_RefTitleMgr != null)
            {
                if (sz.Contains("Create Success.") == true)
                    m_RefTitleMgr.MessageOnOff("가입 성공");
                else if (sz.Contains("ID does exist.") == true)
                    m_RefTitleMgr.MessageOnOff("중복된 ID가 존재합니다.");
                else if (sz.Contains("Nickname does exist.") == true)
                    m_RefTitleMgr.MessageOnOff("중복된 별명이 존재합니다.");
                else
                    m_RefTitleMgr.MessageOnOff(sz);
            }
        }
        else
        {
            if (m_RefTitleMgr != null)
                m_RefTitleMgr.MessageOnOff("가입 실패 : " + a_www.error);

        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;

    }
}
