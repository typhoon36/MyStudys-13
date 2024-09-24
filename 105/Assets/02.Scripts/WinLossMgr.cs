using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLossMgr : MonoBehaviour
{
    [HideInInspector] public double m_CheckWinTime = 2.0f;  //라운드 시작 후 승패 판정은 2초후부터 하기 위해...
    int IsRoomBuf_Team1Win = 0; //정확히 한번만 ++ 시키기 위한 Room 기준의 버퍼 변수
    [HideInInspector] public int m_Team1Win = 0;         //블루팀 승리 카운트
    int IsRoomBuf_Team2Win = 0; //정확히 한번만 ++ 시키기 위한 Room 기준의 버퍼 변수
    [HideInInspector] public int m_Team2Win = 0;         //블랙팀 승리 카운트

    ExitGames.Client.Photon.Hashtable m_Team1WinProps =
                            new ExitGames.Client.Photon.Hashtable();
    ExitGames.Client.Photon.Hashtable m_Team2WinProps =
                            new ExitGames.Client.Photon.Hashtable();

    //--- 싱글턴 패턴
    public static WinLossMgr Inst = null;

    void Awake()
    {
        Inst = this;
    }
    //--- 싱글턴 패턴

    // Start is called before the first frame update
    void Start()
    {
        //----- CustomProperties 초기화
        InitTeam1WinProps();
        InitTeam2WinProps();
        //----- CustomProperties 초기화
    }

    // Update is called once per frame
    void Update()
    {

    }

    //한쪽팀이 전멸했는지 체크하고 승리 / 패배를 감시하고 처리해 주는 함수
    public void WinLossObserver(GameMgr a_GmMgr)
    {
        if (a_GmMgr == null)
            return;

        //---- 승리 / 패배 체크
        if(GameMgr.m_GameState == GameState.GS_Playing)
        { //GS_Ready 상태의 중계가 좀 늦게와서 한쪽이 전멸 상태라는 걸 몇번 체크할 수는 있다.
            m_CheckWinTime -= Time.deltaTime;
            if(m_CheckWinTime <= 0.0f) //게임이 시작된 후 2초 뒤부터 판정을 시작하기 위한 부분
            {
                CheckAliveTeam(a_GmMgr);
            }
        }//if(GameMgr.m_GameState == GameState.GS_Playing)

        if (a_GmMgr.m_WinLossCount != null)
            a_GmMgr.m_WinLossCount.text = "<color=Blue>" + "Team1 : " +
                                            m_Team1Win.ToString() + " 승 " + "</color> / "
                                            + "<color=Black>" + "Team2 : " +
                                            m_Team2Win.ToString() + " 승 " + "</color>";

        if(5 <= (m_Team1Win + m_Team2Win)) // 5Round 까지 모두 플레이된 상황이라면...
        {
            //Game Over 처리
            if(PhotonNetwork.IsMasterClient == true)
               a_GmMgr.SendGState(GameState.GS_GameEnd);

            if(a_GmMgr.m_GameEndText != null)
            {
                a_GmMgr.m_GameEndText.gameObject.SetActive(true);
                if (m_Team1Win < m_Team2Win)
                    a_GmMgr.m_GameEndText.text = "<color=Black>" + "블랙팀 승" + "</color>";
                else
                    a_GmMgr.m_GameEndText.text = "<color=Blue>" + "블루팀 승" + "</color>";
            }

            if (a_GmMgr.m_WaitTmText != null)
                a_GmMgr.m_WaitTmText.gameObject.SetActive(false);

            return;

        }//if(5 <= (m_Team1Win + m_Team2Win)) // 5Round 까지 모두 플레이된 상황이라면...

        //--- 한 Round가 끝나고 다음 Round의 게임을 시작 시키기 위한 부분...
        //모든 탱크 GS_Ready 상태일 때 모두 탱크 대기 상태로 만들기...
        if(a_GmMgr.m_OldState != GameState.GS_Ready && GameMgr.m_GameState == GameState.GS_Ready)
        {
            GameObject[] tanks = GameObject.FindGameObjectsWithTag("TANK");
            foreach(GameObject tank in tanks)
            {
                TankDamage tankDamage = tank.GetComponent<TankDamage>();
                if (tankDamage != null)
                    tankDamage.ReadyStateTank(); //다음 라운드 준비 --> 1
            }
        }
        a_GmMgr.m_OldState = GameMgr.m_GameState;
        //--- 한 Round가 끝나고 다음 Round의 게임을 시작 시키기 위한 부분...

    }//public void WinLossObserver(GameMgr a_GmMgr)

    //한쪽팀이 전멸했는지 체크하는 함수
    void CheckAliveTeam(GameMgr a_GmMgr)
    {
        int a_Tm1Count = 0;
        int a_Tm2Count = 0;
        int rowTm1 = 0;
        int rowTm2 = 0;
        string a_PlrTeam = "blue";  //Player Team

        GameObject[] tanks = GameObject.FindGameObjectsWithTag("TANK");

        Player[] players = PhotonNetwork.PlayerList;
        foreach(Player a_player in players)
        {
            if (a_player.CustomProperties.ContainsKey("MyTeam") == true)
                a_PlrTeam = (string)a_player.CustomProperties["MyTeam"];

            TankDamage tankDamage = null;
            foreach(GameObject tank in tanks)
            {
                TankDamage a_tankDmg = tank.GetComponent<TankDamage>();
                //탱크의 playerId가 포탄의 playerID와 동일한지 판단
                if (a_tankDmg == null)
                    continue;

                if(a_tankDmg.PlayerId == a_player.ActorNumber)
                {
                    tankDamage = a_tankDmg;
                    break;
                }
            }//foreach(GameObject tank in tanks)

            if(a_PlrTeam == "blue")
            {
                if (tankDamage != null && 0 < tankDamage.currHp)
                    rowTm1 = 1; //팀1 중에 한명이라도 살아 있다는 의미
                a_Tm1Count++;   //이 방에 남아 있는 팀1의 플레이어 수
            }
            else if(a_PlrTeam == "black")
            {
                if (tankDamage != null && 0 < tankDamage.currHp)
                    rowTm2 = 1;  //팀2 중에 한명이라도 살아 있다는 의미
                a_Tm2Count++;    //이 방에 남아 있는 팀2의 플에이어 수
            }

        }//foreach(Player a_player in players)

        a_GmMgr.m_GoWaitGame = 4.0f; //다시 4초후에 게임이 시작되도록...

        if (0 < rowTm1 && 0 < rowTm2) //양 팀이 모두 한명 이상 살아 있다는 의미
            return;

        if (5 <= (m_Team1Win + m_Team2Win))
            return;     //5Round까지 모두 진행 했으면 체크할 필요 없음

        if (PhotonNetwork.IsMasterClient == false)
            return;     //승리 패배 값의 중계는 마스터 클라이언트만 하겠다는 의미

        a_GmMgr.SendGState(GameState.GS_Ready);

        if(rowTm1 == 0) //팀 1 전멸 상태
        {
            if(-99999.0f < m_CheckWinTime) //한번만 ++ 시키기 위한 용도
            {
                m_Team2Win++;
                //팀1이 모두 나가버린 경우 강제 승리 처리
                if (GameMgr.m_GameState != GameState.GS_GameEnd && a_Tm1Count <= 0)
                    m_Team2Win = 5 - m_Team1Win;

                //여러번 발생하더라도 아직도 업데이트가
                //안된 상태이기 때문에 이전 값에서 추가될 것이다.
                IsRoomBuf_Team2Win = m_Team2Win;
                m_CheckWinTime = -150000.0f;
            }
            SendTeam2Win(IsRoomBuf_Team2Win);
        }
        else if(rowTm2 == 0) //팀 2 전멸 상태
        {
            if (-99999.0f < m_CheckWinTime) //한번만 ++ 시키기 위한 용도
            {
                m_Team1Win++;

                //팀2가 모두 나가버린 경우 강제 승리 처리
                if (GameMgr.m_GameState != GameState.GS_GameEnd && a_Tm2Count <= 0)
                    m_Team1Win = 5 - m_Team2Win;

                IsRoomBuf_Team1Win = m_Team1Win;
                m_CheckWinTime = -150000.0f;
            }
            SendTeam1Win(IsRoomBuf_Team1Win);
        }

    }//void CheckAliveTeam(GameMgr a_GmMgr)

 #region ------ Team1 Win Count
    void InitTeam1WinProps()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        m_Team1WinProps.Clear();
        m_Team1WinProps.Add("Team1Win", 0);
        PhotonNetwork.CurrentRoom.SetCustomProperties(m_Team1WinProps);
    }

    void SendTeam1Win(int a_WinCount)
    {
        if(m_Team1WinProps == null)
        {
            m_Team1WinProps = new ExitGames.Client.Photon.Hashtable();
            m_Team1WinProps.Clear();
        }

        if (m_Team1WinProps.ContainsKey("Team1Win") == true)
            m_Team1WinProps["Team1Win"] = a_WinCount;
        else
            m_Team1WinProps.Add("Team1Win", a_WinCount);

        PhotonNetwork.CurrentRoom.SetCustomProperties(m_Team1WinProps);
    }
#endregion  //--------------- Team1 Win Count

#region --------------- Team2 Win Count
    void InitTeam2WinProps()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        m_Team2WinProps.Clear();
        m_Team2WinProps.Add("Team2Win", 0);
        PhotonNetwork.CurrentRoom.SetCustomProperties(m_Team2WinProps);
    }

    void SendTeam2Win(int a_WinCount)
    {
        if (m_Team2WinProps == null)
        {
            m_Team2WinProps = new ExitGames.Client.Photon.Hashtable();
            m_Team2WinProps.Clear();
        }

        if (m_Team2WinProps.ContainsKey("Team2Win") == true)
            m_Team2WinProps["Team2Win"] = a_WinCount;
        else
            m_Team2WinProps.Add("Team2Win", a_WinCount);

        PhotonNetwork.CurrentRoom.SetCustomProperties(m_Team2WinProps);
    }
#endregion  //--------------- Team2 Win Count

}
