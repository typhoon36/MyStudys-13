using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//hp 에너지 동기화가 안맞는 현상 (해결방법 : IsMine 기준으로 판정하고 중계한다.)
//정밀하게 총알 원격 충돌 처리를 하려면 원격지 탱크(아바타 탱크)인 경우, 총알을 맞았을 때
//IsMine으로 총알을 맞았다는 RPC 함수를 쏘고
//(여기서 RPC 함수에 총알 고유번호를 같이 보내서 중복 데미지가 들어가지 않도록 처리 해야 한다.)
//IsMine에서 Hp 데미지 계산을 해 준 후 Hp를 중계하여 동기화를 맞춰줘야 한다.

public class TankDamage : MonoBehaviourPunCallbacks, IPunObservable
{
    //탱크 폭파 후 투명 처리를 위한 MeshRenderer 컴포넌트 배열
    private MeshRenderer[] renderers;

    //탱크 폭발 효과 프리팹을 연결할 변수
    private GameObject expEffect = null;

    //탱크의 초기 생명치
    private int initHp = 200;
    //탱크의 현재 생명치
    [HideInInspector] public int currHp = 200;
    int NetHp = 200;    //아바타 탱크(원격지 탱크)들의 Hp 값을 동기화 시켜주기 위한 변수
    //아바타 탱크 입장에서는 네트웍으로 Hp 값을 받아서 죽는 시점을 파악해서 사망처리 하기 위해...

    //탱크 하위의 Canvas 객체를 연결할 변수
    public Canvas hudCanvas;
    //Filled 타입의 ImageUI 항목을 연결할 변수
    public Image hpBar;

    PhotonView pv = null;

    //--- Kill Count 동기화에 필요한 변수
    //탱크 HUD에 표시할 스코어 Text UI 항목
    public Text txtKillCount;

    //플레이어 ID(고유번호) 저장하는 변수
    [HideInInspector] public int PlayerId = -1;

    //적 탱크 파괴 스코어를 CustomProperties를 통해 중계하기 위한 변수들...
    int m_KillCount = 0;    //모든 컴퓨터의 내 탱크들의 변수
    int m_Cur_LAttId = -1;  //누가 마지막 공격(막타) 했는지? Last Attack Id

    ExitGames.Client.Photon.Hashtable KillProps = 
                                        new ExitGames.Client.Photon.Hashtable();
    //--- Kill Count 동기화에 필요한 변수

    [HideInInspector] public float m_ReSetTime = 0.0f;  //부활시간딜레이 
    //게임 시작후에도 딜레이 주기 10초동안

    int m_StCount = 0;
    Vector3 m_StPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
        PlayerId = pv.Owner.ActorNumber;    //플레이어의 고유번호를 하나 저장해 둠

        //탱크 모델의 모든 MeshRenderer 컴포넌트를 추출한 후 배열에 할당
        renderers = GetComponentsInChildren<MeshRenderer>();

        //현재 생명치를 초기 생명치로 초기값 설정
        currHp = initHp;

        //탱크 폭발 시 생성시킬 폭발 효과를 로드
        expEffect = Resources.Load<GameObject>("ExplosionMobile");

        //Filled 이미지 색상을 녹색으로 설정
        hpBar.color = Color.green;

        //--- CustomProperties 초기화
        InitCustomProperties(pv);
        //--- CustomProperties 초기화
    }//void Start()

    int m_UpdateCk = 2;

    // Update is called once per frame
    void Update()
    {
        //탱크는 방에 처음 입장하면 
        //우선 탱크의 상태를 파괴된 이후처럼...
        //보이지 않게 하고 모두 Ready 상태가 되었을 대 시작하게 한다.
        //타이밍 상 모두 Update를 돌고난 후에 적용해야 UI가 깨지지 않는다.(이상현상)
        if(0 < m_UpdateCk)
        {
            m_UpdateCk--;
            if(m_UpdateCk <= 0)
            {
                ReadyStateTank();
            }
        }
        //탱크 방에 처음 입장하시 처리 부분 

        if (0.0f < m_ReSetTime)
            m_ReSetTime -= Time.deltaTime;

        if (PhotonNetwork.CurrentRoom == null ||
            PhotonNetwork.LocalPlayer == null)
            return;
        //동기화 가능한 상태 일때만 업데이트를 계산해 준다.

        //if (0 < m_StCount)
        //{
        //    this.transform.position = m_StPos;
        //    m_StCount--;
        //}

        //--- 원격 플레이어(아바타 탱크 입장) 일 때 동기화 코드
        if (pv.IsMine == false)
        { //원격 플레이어(아바타 탱크 입장) 일 때 수행
            AvataUpdate();

            ReceiveKillCount(); //아바타 탱크들 입장에서 KillCount 중계 방아옴
        }
        //--- 원격 플레이어(아바타 탱크 입장) 일때 동기화 코드

        if (txtKillCount != null)
            txtKillCount.text = m_KillCount.ToString(); //킬 카운트 UI 갱신
    }

    public void ReadyStateTank()
    {
        if (GameMgr.m_GameState != GameState.GS_Ready)
            return;

        StartCoroutine(this.WaitReadyTank());
    }

    //게임 시작 대기...
    IEnumerator WaitReadyTank()
    {
        //HUD를 비활성화
        hudCanvas.enabled = false;

        //탱크 투명 처리
        SetTankVisible(false);

        while(GameMgr.m_GameState == GameState.GS_Ready)
        {
            yield return null;
        }

        //탱크 특정한 위치에 리스폰 되도록...
        float pos = Random.Range(-100.0f, 100.0f);
        Vector3 a_SitPos = new Vector3(pos, 20.0f, pos);

        string a_TeamKind = ReceiveSelTeam(pv.Owner);   //자기 소속 팀 받아오기
        int a_SitPosInx = ReceiveSitPosInx(pv.Owner);   //자기 자리 번호 받아오기
        if(0 <= a_SitPosInx && a_SitPosInx < 4)
        {
            if(a_TeamKind == "blue")
            {
                a_SitPos = GameMgr.m_Team1Pos[a_SitPosInx];
                this.gameObject.transform.eulerAngles =
                                    new Vector3(0.0f, 201.0f, 0.0f);
            }
            else if(a_TeamKind == "black")
            {
                a_SitPos = GameMgr.m_Team2Pos[a_SitPosInx];
                this.gameObject.transform.eulerAngles =
                                    new Vector3(0.0f, 19.5f, 0.0f);
            }
        }//if(0 <= a_SitPosInx && a_SitPosInx < 4)

        this.gameObject.transform.position = a_SitPos;

        //탱크 특정한 위치에 리스폰 되도록...

        //Filled 이미지 초기값으로 환원
        hpBar.fillAmount = 1.0f;
        //Filled 이미지 색상을 녹색으로 설정
        hpBar.color = Color.green;
        //HUD 활성화
        hudCanvas.enabled = true;

        if (pv != null && pv.IsMine == true)    //리스폰 시 생명 초기값 설정
            currHp = initHp;

        //탱크를 다시 보이게 처리
        SetTankVisible(true);

        m_StPos = a_SitPos;
        m_StCount = 5;

    }//IEnumerator WaitReadyTank()

    void OnTriggerEnter(Collider coll)
    {
        //충돌한 Collider의 태크 비교
        if(currHp > 0 && coll.tag == "CANNON")
        {
            int a_Att_Id = -1;
            string a_AttTeam = "blue";
            Cannon a_RefCannon = coll.gameObject.GetComponent<Cannon>();
            if (a_RefCannon != null)
            {
                a_Att_Id = a_RefCannon.AttackerId;
                a_AttTeam = a_RefCannon.AttackerTeam;
            }

            TakeDamage(a_Att_Id, a_AttTeam);

            //currHp -= 20;

            ////현재 생명치 백분율 = (현재 생명치) / (초기 생명치)
            //hpBar.fillAmount = (float)currHp / (float)initHp;

            ////생명 수치에 따라 Filled 이미지의 색상을 변경
            //if(hpBar.fillAmount <= 0.4f)
            //    hpBar.color = Color.red;
            //else if(hpBar.fillAmount <= 0.6f)
            //    hpBar.color = Color.yellow;

            //if(currHp <= 0)
            //{
            //    StartCoroutine(this.ExplosionTank());
            //}
        }
    }

    public void TakeDamage(int AttackerId = -1, string a_AttTeam = "blue")
    {
        //자기가 쏜 총알은 자신이 맞으면 안되도록 처리...
        if (AttackerId == PlayerId)
            return;

        if (currHp <= 0.0f)
            return;

        //피격연출

        if (pv.IsMine == false) //원격지 탱크(아바타)면 제외
            return;

        if (0.0f < m_ReSetTime) //게임 시작 후 10초 동안 딜레이 주기
            return;

        string a_DamageTeam = "blue";
        if (pv.Owner.CustomProperties.ContainsKey("MyTeam") == true)
            a_DamageTeam = (string)pv.Owner.CustomProperties["MyTeam"];

        //지금 데미지를 받는 탱크가 AttackerId 공격자 팀과 
        //다른 팀일때만 데미지가 들어가도록 처리
        if (a_AttTeam == a_DamageTeam)
            return;

        //pv.IsMine 일 때
        m_Cur_LAttId = AttackerId;
        currHp -= 20;
        if (currHp < 0)
            currHp = 0;

        //현재 생명치 백분율 = (현재 생명치) / (초기 생명치)
        hpBar.fillAmount = (float)currHp / (float)initHp;

        //생명 수치에 따라 Filled 이미지의 색상을 변경
        if (hpBar.fillAmount <= 0.4f)
            hpBar.color = Color.red;
        else if (hpBar.fillAmount <= 0.6f)
            hpBar.color = Color.yellow;
        else
            hpBar.color = Color.green;

        if (currHp <= 0)  //죽는 처리 (아바타 탱크들은 중계 받아서 처리)
        {
            //IsMine 기준에서 죽는 처리
            StartCoroutine(this.ExplosionTank());
        }
    }

    //폭발 효과 생성 및 리스폰 코루틴 함수
    IEnumerator ExplosionTank()
    {
        //폭발 효과 생성
        GameObject effect = GameObject.Instantiate(expEffect,
                                            transform.position, 
                                            Quaternion.identity);
        Destroy(effect, 3.0f);

        //HUD를 비활성화
        hudCanvas.enabled = false;

        //탱크 투명 처리
        SetTankVisible(false);

        yield return null;

        //if (pv != null && pv.IsMine == true)  //되살리는 건 IsMine 일때만 하고
        //{
        //    //10초 동안 기다렸다가 활성화하는 로직을 수행
        //    yield return new WaitForSeconds(10.0f);

        //    //Filled 이미지 초깃값으로 환원
        //    hpBar.fillAmount = 1.0f;
        //    //Filled 이미지 색상을 녹색으로 설정
        //    hpBar.color = Color.green;
        //    //HUD 활성화
        //    hudCanvas.enabled = true;

        //    //리스폰 시 생명 초깃값 설정
        //    currHp = initHp;
        //    //탱크를 다시 보이게 처리
        //    SetTankVisible(true);
        //}
        //else
        //{
        //    //아바타 탱크들일 때는 중계 받아서 되살리겠다는 뜻
        //    yield return null;  //한 플레임 정도만 돌고 코루틴 함수 다음을 실행한다.
        //}

    }//IEnumerator ExplosionTank()

    //MeshRenderer를 활성화/비활성화하는 함수
    void SetTankVisible(bool isVisible)
    {
        foreach(MeshRenderer _renderer in renderers)
        {
            _renderer.enabled = isVisible;
        }

        Rigidbody[] a_Rigs = GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody _Rigd in a_Rigs)
        {
            _Rigd.isKinematic = !isVisible;
        }

        BoxCollider[] a_BoxColls = this.GetComponentsInChildren<BoxCollider>(true);
        foreach (BoxCollider _BoxColl in a_BoxColls)
        {
            _BoxColl.enabled = isVisible;
        }

        if (isVisible == true)
            m_ReSetTime = 10.0f;

    }//void SetTankVisible(bool isVisible)

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)  //IsMine쪽 정보 송신
        {
            stream.SendNext(m_Cur_LAttId); //이번에 -20 깍인 게 누구 때문인지 같이 보내준다는 의도
            stream.SendNext(currHp);
        }
        else //원격 플레이어(아바타)의 정보 수신
        {
            m_Cur_LAttId = (int)stream.ReceiveNext(); //아바타들도 누구 때문에 hp가 깍인 건지 알게 한다.
            NetHp = (int)stream.ReceiveNext();
            //아바타 입장에서 사망 시점을 알기 위해 NetHp 라는 변수를 따로 만들어서
            //IsMine에서 송신해 준 Hp 값을 받았다.
        }
    }// public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)

    void AvataUpdate()  //원격지 플레이어 Hp Update 처리 함수
    {
        if(0 < currHp)
        {
            currHp = NetHp;

            //현재 생명치 백분율 = (현재 생명치) / (초기 생명치)
            hpBar.fillAmount = (float)currHp / (float)initHp;

            //생명 수치에 따라 Filled 이미지의 색상을 변경
            if (hpBar.fillAmount <= 0.4f)
                hpBar.color = Color.red;
            else if (hpBar.fillAmount <= 0.6f)
                hpBar.color = Color.yellow;
            else
                hpBar.color = Color.green;

            if(currHp <= 0) //죽는 처리 (아바타 탱크들은 중계 받아서 처리)
            {
                currHp = 0;

                if(0 <= m_Cur_LAttId)  //공격자 Id가 유효할 때
                { //지금 Hp가 깎이게 해서 사망에 이르게 한 탱크가 누구인지?
                    // 지금 죽은 탱크 입장에서는
                    // 공격자의 AttackerId (<--- IsMine)을 찾으려면 
                    // 죽은 탱크 아바타들 중에서 AttackerId (<--- IsMine)을 찾아서
                    // KillCount를 증가시켜 줘야 한다.
                    // 자신을 파괴시킨 적 탱크의 스코어를 증가시키는 함수를 호출
                    SaveKillCount(m_Cur_LAttId);
                }

                //IsMine 기준에서 죽는 처리
                StartCoroutine(this.ExplosionTank());
            }

        } //if(0 < currHp)
        else //if(currHp <= 0) 죽어 있는 상황에서
        { //죽어 있을 때 계속 NetHp는 0으로 계속 들어오게 되고
            //되살려야 하는 상황 처리
            currHp = NetHp;
            if((int)(initHp * 0.95f) < currHp) //이번에 들어온 Hp가 최대 에너지가 들어오면
            {   //되살려야 하는 상황으로 판단하겠다는 뜻

                //Filled 이미지 초기값으로 환원
                hpBar.fillAmount = 1.0f;
                //Filled 이미지 색상을 녹색으로 설정
                hpBar.color = Color.green;
                //HUD 활성화
                hudCanvas.enabled = true;

                //리스폰 시 새 생명 초기값 설정
                currHp = initHp;
                //탱크를 다시 보이게 처리
                SetTankVisible(true);

            }//if ((int)(initHp * 0.95f) < currHp) //이번에 들어온 Hp가 최대 에너지가 들어오면
        }//else //if(currHp <= 0) 죽어 있는 상황에서

    }//void AvataUpdate()  //원격지 플레이어 Hp Update 처리 함수

    //자신을 파괴시킨 적 탱크를 검색해 스코어를 증가시키는 함수
    void SaveKillCount(int AttacketId)
    {
        //TANL 테크로 지정된 모든 탱크를 가져와 배열에 저장
        GameObject[] tanks = GameObject.FindGameObjectsWithTag("TANK");
        foreach(GameObject tank in tanks)
        {
            var tankDamage = tank.GetComponent<TankDamage>();
            if(tankDamage != null && tankDamage.PlayerId == AttacketId)
            { //탱크의 PlayerId가 포탄의 AttackerId와 동일한지 판단
                if(tankDamage.IncKillCount() == true)
                {
                    return;
                }
            }//if(tankDamage != null && tankDamage.PlayerId == AttacketId)
        }//foreach(GameObject tank in tanks)
    }// void SaveKillCount(int AttacketId)

    public bool IncKillCount() //때린 탱크 IsMine 입장에서 이 함수가 호출되어야 한다.
    {
        if(pv != null && pv.IsMine == true)
        {
            //IsMine 한군데서만 KillCount를 증가시키는 이류는
            //IsMine도 증가시키고 아바타도 KillCount를 증가시켜 중계하다보면
            //KillCount가 어긋날 수 있기 때문이다.
            m_KillCount++;

            //IsMone일때만 브로드 캐스팅(중계) <-- CustormProperties에서는 이걸 해 줘야 중계 된다. 
            SendKillCount(m_KillCount);

            return true;
        }

        return false;
    }

    void InitCustomProperties(PhotonView pv)
    {  //버퍼를 미리 만들어 놓기 위한 함수
        if(pv != null && pv.IsMine == true)
        { //pv.IsMine == true 내가 조정하고 있는 탱크이고 스폰시점에...
            KillProps.Clear();
            KillProps.Add("KillCount", 0);
            pv.Owner.SetCustomProperties(KillProps);
        }
    }//void InitCustomProperties(PhotonView pv)

    void SendKillCount(int a_KillCount = 0)
    {
        if (pv == null)
            return;

        if (pv.IsMine == false) //IsMine 일 때만 전송 하겠다는 뜻
            return;

        if (KillProps == null)
        {
            KillProps = new ExitGames.Client.Photon.Hashtable();
            KillProps.Clear();
        }

        if (KillProps.ContainsKey("KillCount") == true)
            KillProps["KillCount"] = a_KillCount;
        else
            KillProps.Add("KillCount", a_KillCount);

        pv.Owner.SetCustomProperties(KillProps);

    }//void SendKillCount(int a_KillCount = 0)

    void ReceiveKillCount() //원격지 탱크들이 KillCount 받아서 처리하는 부분
    {
        if(pv == null)
            return;

        if(pv.IsMine == true)   //원격지 탱크들만 받기 위해서...
            return;

        if(pv.Owner == null)
            return;

        if(pv.Owner.CustomProperties.ContainsKey("KillCount") == true)
        {
            m_KillCount = (int)pv.Owner.CustomProperties["KillCount"];
        }
    }//void ReceiveKillCount() 

    string ReceiveSelTeam(Player a_Player) //using Photon.Realtime;
    {
        string a_TeamKind = "blue";

        if (a_Player == null)
            return a_TeamKind;

        if (a_Player.CustomProperties.ContainsKey("MyTeam") == true)
            a_TeamKind = (string)a_Player.CustomProperties["MyTeam"];

        return a_TeamKind;
    }

    int ReceiveSitPosInx(Player a_Player)
    {
        int a_SitIdx = -1;

        if (a_Player == null)
            return a_SitIdx;

        if (a_Player.CustomProperties.ContainsKey("SitPosInx") == true)
            a_SitIdx = (int)a_Player.CustomProperties["SitPosInx"];

        return a_SitIdx;
    }
}
