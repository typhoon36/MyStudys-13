using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MonType
{
    Skeleton = 0,
    Alien,
    Count
}

public class MonsterCtrl : MonoBehaviour
{
    public MonType monType;

    #region HP
    public Image HP_Bar;
    float m_MaxHP = 100.0f; //최대 HP
    float m_CurHP = 100.0f; //현재 HP
    #endregion



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

    // Start is called before the first frame update
    void Start()
    {
        m_RefAnimation = GetComponentInChildren<Animation>();
        m_RefAnimator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_CurState == AnimState.die)  //죽었으면...
            return;

        MonStateUpdate();
        MonActionUpdate();
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
                    break;
                }
                else if(m_CacDist <= m_TraceDist) //추적거리 범위 이내로 들어왔는지 확인
                {
                    MonState = AnimState.trace;
                    m_AggroTarget = a_Players[i].gameObject;    //타겟 설정
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
            m_RefAnimator.ResetTrigger(m_PreState.ToString());

            if (0 < CrossTime)
                m_RefAnimator.SetTrigger(newState.ToString());
            else
            {
                string a_AnimName = anim.Idle.name;

                if(newState == AnimState.die)
                    a_AnimName = anim.Die.name;

                m_RefAnimator.Play(a_AnimName, -1 ,0.0f);
            }

        }


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

               

                //소수점 한동작이 몇프로 진행 되었는지 계산 변수
                m_CacRate = m_NormalTime - (int)m_NormalTime;

                //공격 애니메이션 끝부분이 아닐 때 (공격애니메시녀 중이라는 뜻)
                if (m_CacRate < 0.95f)
                    return true;
            }
        }
        if(m_RefAnimator != null)
        {
           AnimatorStateInfo animatorStateInfo = m_RefAnimator.GetCurrentAnimatorStateInfo(0);

            if(animatorStateInfo.IsName(anim.Attack1.name))
            {
                m_NormalTime = animatorStateInfo.normalizedTime % 1.0f;

                if(m_NormalTime < 0.95f)
                    return true;
            }
        }
        return false;

    }//public bool IsAttackAnim()

    public void Event_AttHit() //애니메이션 이벤트 함수로 호출
    {
        if(m_AggroTarget == null)
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

    }//public void Event_AttHit() //애니메이션 이벤트 함수로 호출


    public void TakeDamage(GameObject a_Attacker, float a_Dmg = 10.0f)
    {
        if(m_CurHP <= 0.0f) return;


        m_CurHP -= a_Dmg;

        if(m_CurHP <= 0.0f)
            m_CurHP = 0.0f;
       
        HP_Bar.fillAmount = m_CurHP / m_MaxHP;

        Vector3 a_CacPos = transform.position;
        a_CacPos.y += 2.65f;
        GameMgr.Inst.SpawnDamageText((int)a_Dmg, a_CacPos);


        if(m_CurHP <=0.0f)
        {
            Destroy(gameObject);
        }


    }
}
