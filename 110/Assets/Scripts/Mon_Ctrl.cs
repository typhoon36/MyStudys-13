using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonType
{
    Skeleton = 0,
    Alien = 1,
    Cnt
}


public class Mon_Ctrl : MonoBehaviour
{
    public MonType m_MonType;

    AnimState m_PreState = AnimState.idle;//--> 애니메이션 변경 변수
    [HideInInspector] public AnimState m_CurState = AnimState.idle;//--> 애니메이션 변경 변수
    AnimState MonState = AnimState.idle; //--> AI에서 사용하는 변수

    public Anim anim;
    Animation m_RefAnim = null;
    Animator m_RefAnimator = null;

    #region AI
    [HideInInspector] public GameObject m_ATarget = null;
    int m_ATargetId = -1;

    Quaternion m_TargetRot;
    Vector3 m_MoveDir = Vector3.zero;
    Vector3 m_CacVLen = Vector3.zero;
    Vector3 m_MoveNStep = Vector3.zero;
    float m_CacDist = 0;
    float m_TraceDist = 7.0f;
    float m_AttackDist = 1.8f;
    float m_MoveVel = 2.0f;
    float m_RotSpeed = 7.0f;
    #endregion

    void Start()
    {
        m_RefAnim  = this.GetComponentInChildren<Animation>();
    }

    void Update()
    {
        if (m_CurState == AnimState.die) return;

        MonStateUpdate();
        MonActionUpdate();
    }

    void MonStateUpdate()
    {
        if (m_ATarget == null)
        {
            GameObject[] a_Players = GameObject.FindGameObjectsWithTag("Player");

            for (int i = 0; i < a_Players.Length; i++)
            {
                m_CacVLen = a_Players[i].transform.position - this.transform.position;
                m_CacVLen.y = 0;
                m_MoveDir = m_CacVLen.normalized;
                m_CacDist = m_CacVLen.magnitude;

                if (m_CacDist <= m_AttackDist)
                {
                    MonState = AnimState.attack;
                    m_ATarget = a_Players[i].gameObject;
                    break;
                }
                else if (m_CacDist <= m_TraceDist)
                {
                    MonState = AnimState.trace;
                    m_ATarget = a_Players[i].gameObject;
                    break;
                }
            }

            if (m_ATarget == null)
            {
                MonState = AnimState.idle;
                m_ATargetId = -1;
            }
        }
        else
        {
            m_CacVLen = m_ATarget.transform.position - this.transform.position;
            m_CacVLen.y = 0;
            m_MoveDir = m_CacVLen.normalized;
            m_CacDist = m_CacVLen.magnitude;

            if (m_CacDist <= m_AttackDist)
                MonState = AnimState.attack;
            else if (m_CacDist <= m_TraceDist)
                MonState = AnimState.trace;
            else
            {
                MonState = AnimState.idle;
                m_ATarget = null;
                m_ATargetId = -1;
            }
        }
    }

    void MonActionUpdate()
    {
        if (m_ATarget == null)
        {
            ChangeAnimState(AnimState.idle, 0.12f);
        }
        else
        {
            if (MonState == AnimState.attack)
            {
                if (0.0001f < m_MoveDir.magnitude)
                {
                    m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRot, Time.deltaTime * m_RotSpeed);
                }

                ChangeAnimState(AnimState.attack, 0.12f);
            }
            else if (MonState == AnimState.trace)
            {
                

                if (0.0001f < m_MoveDir.magnitude)
                {
                    m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRot, Time.deltaTime * m_RotSpeed);
                }

                if (IsAttackAnim() == true) return;

                // 이동
                m_MoveNStep = m_MoveDir * (Time.deltaTime * m_MoveVel);
                m_MoveDir.y = 0;
                transform.position += m_MoveNStep;
                ChangeAnimState(AnimState.trace, 0.12f);
            }
            else if (MonState == AnimState.idle)
            {
                ChangeAnimState(AnimState.idle);
            }
        }
    }

    void ChangeAnimState(AnimState N_State, float CrossTime = 0.0f)
    {
        if (m_PreState == N_State) return;

        if (m_RefAnim != null)
        {
            string a_StrAnim = anim.Idle.name;

            if (N_State == AnimState.trace)
            {
                a_StrAnim = anim.Move.name;
            }
            else if (N_State == AnimState.attack)
            {
                a_StrAnim = anim.Attack1.name;
            }
            else if (N_State == AnimState.die)
            {
                a_StrAnim = anim.Die.name;
            }

            if (0.0f < CrossTime)
            {
                m_RefAnim.CrossFade(a_StrAnim, CrossTime);
            }
            else
            {
                m_RefAnim.Play(a_StrAnim);
            }

            m_PreState = N_State;
            m_CurState = N_State;
        }

    }

    //애니메이션(공격) 멤버 변수
    float m_CacRate = 0;
    float m_NrmTime = 0;

    public bool IsAttackAnim()
    {
        if (m_RefAnim == null) return false;

        if (m_RefAnim.IsPlaying(anim.Attack1.name) == true)
        {
            m_NrmTime = m_RefAnim[anim.Attack1.name].time /
                m_RefAnim[anim.Attack1.name].length;

            m_CacRate = m_NrmTime - (int)m_NrmTime;

            if (m_CacRate < 0.95f)
                return true;

        }

        return false;

    }

    public void EventAttDmg()
    {
        if(m_ATarget == null) return;

        Vector3 a_DistVec = m_ATarget.transform.position - this.transform.position;

        float a_CacLen = a_DistVec.magnitude;
        a_DistVec.y = 0;

        if(Vector3.Dot(a_DistVec.normalized, this.transform.forward) < 0.0f)
            return;
        

        //범위 밖
        if((m_AttackDist * 1.7f) < a_CacLen) return;

        m_ATarget.GetComponent<Hero_Ctrl>().TakeDmg(10);

    }

}
