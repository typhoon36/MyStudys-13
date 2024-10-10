using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum MonType
{
    Skeleton = 0,
    Alien,
    Count
}

public class MonsterCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    PhotonView pv = null;

    public MonType monType;

    //--- Hp 바 표시
    float CurHp = 100;
    float MaxHp = 100;
    float NetHp = 100;  //CurHp 중계용
    public Image ImgHpbar;
    //--- Hp 바 표시

    [HideInInspector] public int m_SpawnIdx = -1; //List<SpawnPos> m_SpawnPos; 의 인덱스

    //몬스터의 현재 상태 정보를 저장할 Enum 변수
    AnimState m_PreState = AnimState.idle;  //애니메이션 변경을 위한 변수
    [HideInInspector] public AnimState m_CurState = AnimState.idle;  //애니메이션 변경을 위한 변수

    AnimState MonState   = AnimState.idle;  //몬스터의 현재 AI 상태를 저장할 Enum 변수
    //AnimSupporter.cs 쪽에 정의되어 있음

    //인스펙터뷰에 표시할 애니메이션 클래스 변수
    public Anim anim;   //AnimSupporter.cs 쪽에 정의되어 있음
    Animation m_RefAnimation = null;    //Skeleton
    Animator m_RefAnimator = null;      //Alien

    //--- Monster AI
    [HideInInspector] public GameObject m_AggroTarget = null;   //공격할 대상
    int m_AggroTgID = -1;   //이 몬스터가 공격해야 할 캐릭터의 고유번호
    Vector3 m_MoveDir = Vector3.zero;   //수평 진행 노멀 방향 벡터
    Vector3 m_CacVLen = Vector3.zero;   //주인공을 향하는 벡터
    float   m_CacDist = 0.0f;           //거리 계산용 변수
    float   m_TraceDist = 7.0f;         //추적 거리
    float   m_AttackDist = 1.8f;        //공격 거리
    Quaternion m_TargetRot;             //회전 계산용 변수
    float   m_RotSpeed = 7.0f;          //초당 회전 속도
    Vector3 m_MoveNextStep = Vector3.zero; //이동 계산용 변수
    float   m_MoveVelocity = 2.0f;      //평명 초당 이동 속도
    //--- Monster AI

    //--- 네트웍 동기화를 위하여...
    Vector3 currPos = Vector3.zero;
    Quaternion currRot = Quaternion.identity;
    int currAnim = 0;
    string m_OldAnim = "";
    //--- 네트웍 동기화를 위하여...

    void Awake()
    {
        pv = GetComponent<PhotonView>();

        //원격 캐릭터의 위치 및 회전 값을 처리할 변수의 초기값 설정 
        currPos = transform.position;
        currRot = transform.rotation;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_RefAnimation = GetComponentInChildren<Animation>();
        m_RefAnimator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //나가는 타이밍에 포톤 정보들이 한플레임 먼저 사라지고
        //LoadScene()이 한플레임 늦게 호출되는 문제 해결볍
        if (PhotonNetwork.CurrentRoom == null)
            return;

        if (m_CurState == AnimState.die)  //죽었으면...
            return;

        if (pv.IsMine == true)
        {   //여기서의 pv.IsMine은 InstantiateRoomObject 로 생성 되었기 때문에 
            //PhotonNetwork.IsMasterClient 로 예상됨
            MonStateUpdate();
            MonActionUpdate();
        }
        else
        {
            Remote_TrUpdate();
            Remote_TakeDamage();
            Remote_Animation();
        }
    }

    void MonStateUpdate()
    {
        if(m_AggroTarget == null)  //어그로 타겟이 존재 하지 않을 경우
        {
            GameObject[] a_Players = GameObject.FindGameObjectsWithTag("Player");
            for(int i = 0; i < a_Players.Length; i++)
            {
                m_CacVLen = a_Players[i].transform.position - this.transform.position;
                m_CacVLen.y = 0.0f;
                m_MoveDir = m_CacVLen.normalized;   //주인공을 향하는 벡터
                m_CacDist = m_CacVLen.magnitude;    //주인공까지의 거리

                if(m_CacDist <= m_AttackDist) //공격거리 범위 이내로 들어왔는지 확인
                {
                    MonState = AnimState.attack;
                    m_AggroTarget = a_Players[i].gameObject;    //타겟 설정
                    //Player[] players = PhotonNetwork.PlayerList;  //using Photon.Realtime;
                    //players[0].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber
                    //== pv.Owner.ActorNumber;    //플레이어의 고유번호를 하나 저장해 둠
                    //m_AggroTgID = a_Players[i].GetComponent<Hero_Ctrl>().pv.ViewID;
                    m_AggroTgID = a_Players[i].GetComponent<Hero_Ctrl>().pv.Owner.ActorNumber;
                    break;
                }
                else if(m_CacDist <= m_TraceDist) //추적거리 범위 이내로 들어왔는지 확인
                {
                    MonState = AnimState.trace;
                    m_AggroTarget = a_Players[i].gameObject;    //타겟 설정
                    //m_AggroTgID = a_Players[i].GetComponent<Hero_Ctrl>().pv.ViewID;
                    m_AggroTgID = a_Players[i].GetComponent<Hero_Ctrl>().pv.Owner.ActorNumber;
                    break;
                }
            }//for(int i = 0; i < a_Players.Length; i++)

            if(m_AggroTarget == null)
            {
                MonState = AnimState.idle;  //몬스터의 상태를 idle 모드로 설정
                m_AggroTgID = -1;
            }
        }
        else  //if(m_AggroTarget != null)  어그로 타겟이 존재할 경우
        {
            m_CacVLen = m_AggroTarget.transform.position - this.transform.position;
            m_CacVLen.y = 0.0f;
            m_MoveDir = m_CacVLen.normalized;   //주인공을 향하는 벡터
            m_CacDist = m_CacVLen.magnitude;    //주인공까지의 거리

            if(m_CacDist <= m_AttackDist)   //공격거리 범위 이내로 들어왔는지 확인
            {
                MonState = AnimState.attack;
            }
            else if(m_CacDist <= m_TraceDist) //추적거리 범위 이내로 들어왔는지 확인 
            {
                MonState = AnimState.trace;
            }
            else
            {
                MonState = AnimState.idle;  //몬스터의 상태를 idle 상태로 설정
                m_AggroTarget = null;
                m_AggroTgID = -1;
            }
        }//else  //if(m_AggroTarget != null)  어그로 타겟이 존재할 경우
    }

    void MonActionUpdate()
    {
        if(m_AggroTarget == null)   //어그로 타겟이 존재하지 않을 경우
        {
            //애니메이션 상태를 Idle 상태로 돌려 놓는다.
            ChangeAnimState(AnimState.idle, 0.12f);
        }
        else //if(m_AggroTarget != null)   //어그로 타겟이 존재 하는 경우
        {
            if(MonState == AnimState.attack) //공격 상태 일때
            {
                if(0.0001f < m_MoveDir.magnitude)
                {
                    m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                                        m_TargetRot, Time.deltaTime * m_RotSpeed);
                }

                ChangeAnimState(AnimState.attack, 0.12f);   //공격 애니메이션 적용
            }
            else if(MonState == AnimState.trace)   //추적상태 일때 
            {
                if (0.0001f < m_MoveDir.magnitude)
                {
                    m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                                        m_TargetRot, Time.deltaTime * m_RotSpeed);
                }

                //아직 공격 애니메이션 중이면 공격 애니가 끝난 경우에만 추적 이동하도록...
                if (IsAttackAnim() == true)
                    return;

                //--- 몬스터 이동 코드
                m_MoveNextStep = m_MoveDir * (m_MoveVelocity * Time.deltaTime);
                m_MoveNextStep.y = 0.0f;
                transform.position += m_MoveNextStep;

                ChangeAnimState(AnimState.trace, 0.12f);  //추적 애니메이션 적용
            }
            else if(MonState == AnimState.idle)
            {
                ChangeAnimState(AnimState.idle, 0.12f);    //숨쉬기 애니메이션 적용
            }
        }//else //if(m_AggroTarget != null)   //어그로 타겟이 존재 하는 경우
    }//void MonActionUpdate()

    //애니메이션 상태 변경 메서드
    void ChangeAnimState(AnimState newState, float CrossTime = 0.0f)
    {
        if (m_PreState == newState)
            return;

        if(m_RefAnimation != null)
        {
            string a_strAnim = anim.Idle.name;
            if (newState == AnimState.idle)
                a_strAnim = anim.Idle.name;
            else if(newState == AnimState.trace)
                a_strAnim = anim.Move.name;
            else if(newState == AnimState.attack)
                a_strAnim = anim.Attack1.name;
            else if(newState == AnimState.die)
                a_strAnim = anim.Die.name;

            if (0.0f < CrossTime)
                m_RefAnimation.CrossFade(a_strAnim, CrossTime);
            else
                m_RefAnimation.Play(a_strAnim);

        }//if(m_RefAnimation != null)

        if(m_RefAnimator != null)
        {
            m_RefAnimator.ResetTrigger(m_PreState.ToString()); //기존에 적용되어 있던 Trigger 변수를 제거

            if (0.0f < CrossTime)
                m_RefAnimator.SetTrigger(newState.ToString());
            else
            {
                string animName = anim.Idle.name;
                if (newState == AnimState.die)
                    animName = anim.Die.name;

                m_RefAnimator.Play(animName, -1, 0);
                //가운데 -1은 Layer Index, 뒤에 0는 처음부터 다시 시작 플레이 시키겠다는 의미
            }
        }//if(m_RefAnimator != null)

        m_PreState = newState;
        m_CurState = newState;
    }//void ChangeAnimState(AnimState newState, float CrossTime = 0.0f)

    //--- 공격 애니 관련 변수
    float m_CacRate = 0.0f;
    float m_NormalTime = 0.0f;
    //--- 공격 애니 관련 변수
    bool IsAttackAnim()  //공격 애니메이션 상태 체크 함수
    {

        if (m_RefAnimation != null)
        {
            if (m_RefAnimation.IsPlaying(anim.Attack1.name) == true)
            {
                m_NormalTime = m_RefAnimation[anim.Attack1.name].time
                                / m_RefAnimation[anim.Attack1.name].length;

                // m_RefAnimation["Attack1h1"].time
                // 어느 정도 플레이가 되고 있는지의 현재 시간값
                // m_RefAnimation["Attack1h1"].length
                // 한동작이 끝날 때까지의 시간값

                //소수점 한동작이 몇프로 진행 되었는지 계산 변수
                m_CacRate = m_NormalTime - (int)m_NormalTime;

                //공격 애니메이션 끝부분이 아닐 때 (공격애니메시녀 중이라는 뜻)
                if (m_CacRate < 0.95f)
                    return true;
            }
        }//if (m_RefAnimation != null)

        if(m_RefAnimator != null)
        {
            // 현재 애니메이션 상태 정보를 가져옵니다 (레이어 인덱스는 보통 0)
            AnimatorStateInfo stateInfo = m_RefAnimator.GetCurrentAnimatorStateInfo(0);

            // 현재 상태가 공격 애니메이션인지 확인합니다
            if (stateInfo.IsName(anim.Attack1.name))
            {
                // 애니메이션의 진행도를 가져옵니다 (0에서 1 사이, 1이면 애니메이션 완료)
                m_NormalTime = stateInfo.normalizedTime % 1.0f;

                // 애니메이션이 아직 끝부분이 아니라면 (95% 미만 진행)
                if (m_NormalTime < 0.95f)
                    return true;
            }
        }//if(m_RefAnimator != null)

        return false;

    }//public bool IsAttackAnim()

    public void Event_AttHit() //애니메이션 이벤트 함수로 호출
    {
        if (pv.IsMine == true)
        {

            if (m_AggroTarget == null)
                return;

            Vector3 a_DistVec = m_AggroTarget.transform.position - transform.position;
            float a_CacLen = a_DistVec.magnitude;
            a_DistVec.y = 0.0f;

            //공격각도 안에 있는 경우
            if (Vector3.Dot(transform.forward, a_DistVec.normalized) < 0.0f)
                return; //90도를 넘는 범위에 있다는 뜻

            //공격 범위 밖에 있는 경우
            if ((m_AttackDist + 1.7f) < a_CacLen)
                return;

            m_AggroTarget.GetComponent<Hero_Ctrl>().TakeDamage(10);

        }//if (pv.IsMine == true)
        else //아바타들 입장에서는
        {
            if (m_AggroTgID < 0)
                return;

            GameObject a_AggroTg = null;
            GameObject[] a_Players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < a_Players.Length; i++)
            {
                //if (m_AggroTgID == a_Players[i].GetComponent<Hero_Ctrl>().pv.ViewID)
                if (m_AggroTgID == a_Players[i].GetComponent<Hero_Ctrl>().pv.Owner.ActorNumber)
                {
                    a_AggroTg = a_Players[i];
                    break;
                }
            }//for(int i = 0; i < a_Players.Length; i++)

            if (a_AggroTg == null)
                return;

            Vector3 a_DistVec = a_AggroTg.transform.position - transform.position;
            float a_CacLen = a_DistVec.magnitude;
            a_DistVec.y = 0.0f;

            //공격각도 안에 있는 경우
            if (Vector3.Dot(transform.forward, a_DistVec.normalized) < 0.0f)
                return; //90도를 넘는 범위에 있다는 뜻

            //공격 범위 밖에 있는 경우
            if ((m_AttackDist + 1.7) < a_CacLen)
                return;

            a_AggroTg.GetComponent<Hero_Ctrl>().TakeDamage(10);
        }

    }//public void Event_AttHit() //애니메이션 이벤트 함수로 호출

    public void TakeDamage(GameObject a_Attacker, float a_Damage = 10.0f)
    {
        if (CurHp <= 0.0f)
            return;

        if (pv.IsMine == true)  //실제 데미지는 IsMine일 때만 깍아준다. IsMine이 아니면 연출만...
        {
            CurHp -= a_Damage;
            if (CurHp < 0.0f)
                CurHp = 0.0f;

            ImgHpbar.fillAmount = CurHp / MaxHp;
        }

        Vector3 a_CacPos = this.transform.position;
        a_CacPos.y += 2.65f;
        GameMgr.Inst.SpawnDamageText((int)a_Damage, a_CacPos);

        if(pv.IsMine == true) //사망 처리도 IsMine 일때만... 나머지는 동기화로 처리함
        if(CurHp <= 0.0f)   //사망처리
        {
            //Destroy(gameObject);
            CreateItem();
            PhotonNetwork.Destroy(this.gameObject); //삭제 동기화
        }
    }//public void TakeDamage(GameObject a_Attacker, float a_Damage = 10.0f)

    void Remote_TrUpdate()
    {
        if(5.0f < (transform.position - currPos).magnitude)
        {
            transform.position = currPos;
        }
        else
        {
            //원격 플레이어의 Monster를 수신 받은 위치까지 부드럽게 이동시킴
            transform.position = Vector3.Lerp(transform.position, currPos, Time.deltaTime * 10.0f);
        }

        //원격 플레이어의 Monter를 수신 받은 각도만큼 부드럽게 회전시킴
        transform.rotation = Quaternion.Slerp(transform.rotation, currRot, Time.deltaTime * 10.0f);
    }

    void Remote_TakeDamage()  //원격지 컴퓨터에서 Hp 동기화 함수
    {
        if(0.0f < CurHp)
        {
            CurHp = NetHp;
            ImgHpbar.fillAmount = CurHp / (float)MaxHp;

            if(CurHp <= 0.0f)
            {
                CurHp = 0.0f;
                //사망 연출만 해 준다. 
                //실제 삭제는 PhotonNetwork.Destroy(this.gameObject);  함수에 의해 삭제 동기화 됨
            }//if(CurHp <= 0.0f)
        }//if(0.0f < CurHp)
        else
        {
            CurHp = NetHp;
            ImgHpbar.fillAmount = CurHp / (float)MaxHp;
        }
    }//void Remote_TakeDamage()  //원격지 컴퓨터에서 Hp 동기화 함수

    void Remote_Animation()  //원격지 컴퓨터에서 애니메이션 동기화 함수
    {
        ChangeAnimState(m_CurState, 0.12f);
    }

    void CreateItem()
    {
        Vector3 a_Hpos = transform.position;
        a_Hpos.y += 0.1f;
        

        PhotonNetwork.InstantiateRoomObject("DiamondItem/DiamondPrefab", a_Hpos, Quaternion.identity,0);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //로컬 플레이어의 위치 정보 송신
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(CurHp);
            stream.SendNext(m_AggroTgID);

            stream.SendNext((int)m_CurState);
            stream.SendNext(m_SpawnIdx);
        }
        else //원격 플레이어의 위치 정보 수신
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext(); 
            NetHp = (float)stream.ReceiveNext();
            m_AggroTgID = (int)stream.ReceiveNext();

            m_CurState = (AnimState)stream.ReceiveNext();
            m_SpawnIdx = (int)stream.ReceiveNext();
        }
    }//public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)

   
}
