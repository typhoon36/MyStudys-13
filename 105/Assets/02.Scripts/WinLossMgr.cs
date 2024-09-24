using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLossMgr : MonoBehaviour
{
    [HideInInspector] public double m_CheckWinTime = 2.0f;  //���� ���� �� ���� ������ 2���ĺ��� �ϱ� ����...
    int IsRoomBuf_Team1Win = 0; //��Ȯ�� �ѹ��� ++ ��Ű�� ���� Room ������ ���� ����
    [HideInInspector] public int m_Team1Win = 0;         //����� �¸� ī��Ʈ
    int IsRoomBuf_Team2Win = 0; //��Ȯ�� �ѹ��� ++ ��Ű�� ���� Room ������ ���� ����
    [HideInInspector] public int m_Team2Win = 0;         //���� �¸� ī��Ʈ

    ExitGames.Client.Photon.Hashtable m_Team1WinProps =
                            new ExitGames.Client.Photon.Hashtable();
    ExitGames.Client.Photon.Hashtable m_Team2WinProps =
                            new ExitGames.Client.Photon.Hashtable();

    //--- �̱��� ����
    public static WinLossMgr Inst = null;

    void Awake()
    {
        Inst = this;
    }
    //--- �̱��� ����

    // Start is called before the first frame update
    void Start()
    {
        //----- CustomProperties �ʱ�ȭ
        InitTeam1WinProps();
        InitTeam2WinProps();
        //----- CustomProperties �ʱ�ȭ
    }

    // Update is called once per frame
    void Update()
    {

    }

    //�������� �����ߴ��� üũ�ϰ� �¸� / �й踦 �����ϰ� ó���� �ִ� �Լ�
    public void WinLossObserver(GameMgr a_GmMgr)
    {
        if (a_GmMgr == null)
            return;

        //---- �¸� / �й� üũ
        if(GameMgr.m_GameState == GameState.GS_Playing)
        { //GS_Ready ������ �߰谡 �� �ʰԿͼ� ������ ���� ���¶�� �� ��� üũ�� ���� �ִ�.
            m_CheckWinTime -= Time.deltaTime;
            if(m_CheckWinTime <= 0.0f) //������ ���۵� �� 2�� �ں��� ������ �����ϱ� ���� �κ�
            {
                CheckAliveTeam(a_GmMgr);
            }
        }//if(GameMgr.m_GameState == GameState.GS_Playing)

        if (a_GmMgr.m_WinLossCount != null)
            a_GmMgr.m_WinLossCount.text = "<color=Blue>" + "Team1 : " +
                                            m_Team1Win.ToString() + " �� " + "</color> / "
                                            + "<color=Black>" + "Team2 : " +
                                            m_Team2Win.ToString() + " �� " + "</color>";

        if(5 <= (m_Team1Win + m_Team2Win)) // 5Round ���� ��� �÷��̵� ��Ȳ�̶��...
        {
            //Game Over ó��
            if(PhotonNetwork.IsMasterClient == true)
               a_GmMgr.SendGState(GameState.GS_GameEnd);

            if(a_GmMgr.m_GameEndText != null)
            {
                a_GmMgr.m_GameEndText.gameObject.SetActive(true);
                if (m_Team1Win < m_Team2Win)
                    a_GmMgr.m_GameEndText.text = "<color=Black>" + "���� ��" + "</color>";
                else
                    a_GmMgr.m_GameEndText.text = "<color=Blue>" + "����� ��" + "</color>";
            }

            if (a_GmMgr.m_WaitTmText != null)
                a_GmMgr.m_WaitTmText.gameObject.SetActive(false);

            return;

        }//if(5 <= (m_Team1Win + m_Team2Win)) // 5Round ���� ��� �÷��̵� ��Ȳ�̶��...

        //--- �� Round�� ������ ���� Round�� ������ ���� ��Ű�� ���� �κ�...
        //��� ��ũ GS_Ready ������ �� ��� ��ũ ��� ���·� �����...
        if(a_GmMgr.m_OldState != GameState.GS_Ready && GameMgr.m_GameState == GameState.GS_Ready)
        {
            GameObject[] tanks = GameObject.FindGameObjectsWithTag("TANK");
            foreach(GameObject tank in tanks)
            {
                TankDamage tankDamage = tank.GetComponent<TankDamage>();
                if (tankDamage != null)
                    tankDamage.ReadyStateTank(); //���� ���� �غ� --> 1
            }
        }
        a_GmMgr.m_OldState = GameMgr.m_GameState;
        //--- �� Round�� ������ ���� Round�� ������ ���� ��Ű�� ���� �κ�...

    }//public void WinLossObserver(GameMgr a_GmMgr)

    //�������� �����ߴ��� üũ�ϴ� �Լ�
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
                //��ũ�� playerId�� ��ź�� playerID�� �������� �Ǵ�
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
                    rowTm1 = 1; //��1 �߿� �Ѹ��̶� ��� �ִٴ� �ǹ�
                a_Tm1Count++;   //�� �濡 ���� �ִ� ��1�� �÷��̾� ��
            }
            else if(a_PlrTeam == "black")
            {
                if (tankDamage != null && 0 < tankDamage.currHp)
                    rowTm2 = 1;  //��2 �߿� �Ѹ��̶� ��� �ִٴ� �ǹ�
                a_Tm2Count++;    //�� �濡 ���� �ִ� ��2�� �ÿ��̾� ��
            }

        }//foreach(Player a_player in players)

        a_GmMgr.m_GoWaitGame = 4.0f; //�ٽ� 4���Ŀ� ������ ���۵ǵ���...

        if (0 < rowTm1 && 0 < rowTm2) //�� ���� ��� �Ѹ� �̻� ��� �ִٴ� �ǹ�
            return;

        if (5 <= (m_Team1Win + m_Team2Win))
            return;     //5Round���� ��� ���� ������ üũ�� �ʿ� ����

        if (PhotonNetwork.IsMasterClient == false)
            return;     //�¸� �й� ���� �߰�� ������ Ŭ���̾�Ʈ�� �ϰڴٴ� �ǹ�

        a_GmMgr.SendGState(GameState.GS_Ready);

        if(rowTm1 == 0) //�� 1 ���� ����
        {
            if(-99999.0f < m_CheckWinTime) //�ѹ��� ++ ��Ű�� ���� �뵵
            {
                m_Team2Win++;
                //��1�� ��� �������� ��� ���� �¸� ó��
                if (GameMgr.m_GameState != GameState.GS_GameEnd && a_Tm1Count <= 0)
                    m_Team2Win = 5 - m_Team1Win;

                //������ �߻��ϴ��� ������ ������Ʈ��
                //�ȵ� �����̱� ������ ���� ������ �߰��� ���̴�.
                IsRoomBuf_Team2Win = m_Team2Win;
                m_CheckWinTime = -150000.0f;
            }
            SendTeam2Win(IsRoomBuf_Team2Win);
        }
        else if(rowTm2 == 0) //�� 2 ���� ����
        {
            if (-99999.0f < m_CheckWinTime) //�ѹ��� ++ ��Ű�� ���� �뵵
            {
                m_Team1Win++;

                //��2�� ��� �������� ��� ���� �¸� ó��
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
