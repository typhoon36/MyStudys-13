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
    float m_MaxHP = 100.0f; //�ִ� HP
    float m_CurHP = 100.0f; //���� HP
    #endregion



    //������ ���� ���� ������ ������ Enum ����
    AnimState m_PreState = AnimState.idle;  //�ִϸ��̼� ������ ���� ����
    [HideInInspector] public AnimState m_CurState = AnimState.idle;  //�ִϸ��̼� ������ ���� ����

    AnimState MonState   = AnimState.idle;  //������ ���� AI ���¸� ������ Enum ����
    //AnimSupporter.cs �ʿ� ���ǵǾ� ����

    //�ν����ͺ信 ǥ���� �ִϸ��̼� Ŭ���� ����
    public Anim anim;   //AnimSupporter.cs �ʿ� ���ǵǾ� ����
    Animation m_RefAnimation = null;    //Skeleton
    Animator m_RefAnimator = null;      //Alien


    //--- Monster AI
    [HideInInspector] public GameObject m_AggroTarget = null;   //������ ���
    int m_AggroTgID = -1;   //�� ���Ͱ� �����ؾ� �� ĳ������ ������ȣ
    Vector3 m_MoveDir = Vector3.zero;   //���� ���� ��� ���� ����
    Vector3 m_CacVLen = Vector3.zero;   //���ΰ��� ���ϴ� ����
    float   m_CacDist = 0.0f;           //�Ÿ� ���� ����
    float   m_TraceDist = 7.0f;         //���� �Ÿ�
    float   m_AttackDist = 1.8f;        //���� �Ÿ�
    Quaternion m_TargetRot;             //ȸ�� ���� ����
    float   m_RotSpeed = 7.0f;          //�ʴ� ȸ�� �ӵ�
    Vector3 m_MoveNextStep = Vector3.zero; //�̵� ���� ����
    float   m_MoveVelocity = 2.0f;      //��� �ʴ� �̵� �ӵ�
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
        if (m_CurState == AnimState.die)  //�׾�����...
            return;

        MonStateUpdate();
        MonActionUpdate();
    }

    void MonStateUpdate()
    {
        if(m_AggroTarget == null)  //��׷� Ÿ���� ���� ���� ���� ���
        {
            GameObject[] a_Players = GameObject.FindGameObjectsWithTag("Player");
            for(int i = 0; i < a_Players.Length; i++)
            {
                m_CacVLen = a_Players[i].transform.position - this.transform.position;
                m_CacVLen.y = 0.0f;
                m_MoveDir = m_CacVLen.normalized;   //���ΰ��� ���ϴ� ����
                m_CacDist = m_CacVLen.magnitude;    //���ΰ������� �Ÿ�

                if(m_CacDist <= m_AttackDist) //���ݰŸ� ���� �̳��� ���Դ��� Ȯ��
                {
                    MonState = AnimState.attack;
                    m_AggroTarget = a_Players[i].gameObject;    //Ÿ�� ����
                    break;
                }
                else if(m_CacDist <= m_TraceDist) //�����Ÿ� ���� �̳��� ���Դ��� Ȯ��
                {
                    MonState = AnimState.trace;
                    m_AggroTarget = a_Players[i].gameObject;    //Ÿ�� ����
                    break;
                }
            }//for(int i = 0; i < a_Players.Length; i++)

            if(m_AggroTarget == null)
            {
                MonState = AnimState.idle;  //������ ���¸� idle ���� ����
                m_AggroTgID = -1;
            }
        }
        else  //if(m_AggroTarget != null)  ��׷� Ÿ���� ������ ���
        {
            m_CacVLen = m_AggroTarget.transform.position - this.transform.position;
            m_CacVLen.y = 0.0f;
            m_MoveDir = m_CacVLen.normalized;   //���ΰ��� ���ϴ� ����
            m_CacDist = m_CacVLen.magnitude;    //���ΰ������� �Ÿ�

            if(m_CacDist <= m_AttackDist)   //���ݰŸ� ���� �̳��� ���Դ��� Ȯ��
            {
                MonState = AnimState.attack;
            }
            else if(m_CacDist <= m_TraceDist) //�����Ÿ� ���� �̳��� ���Դ��� Ȯ�� 
            {
                MonState = AnimState.trace;
            }
            else
            {
                MonState = AnimState.idle;  //������ ���¸� idle ���·� ����
                m_AggroTarget = null;
                m_AggroTgID = -1;
            }
        }//else  //if(m_AggroTarget != null)  ��׷� Ÿ���� ������ ���
    }

    void MonActionUpdate()
    {
        if(m_AggroTarget == null)   //��׷� Ÿ���� �������� ���� ���
        {
            //�ִϸ��̼� ���¸� Idle ���·� ���� ���´�.
            ChangeAnimState(AnimState.idle, 0.12f);
        }
        else //if(m_AggroTarget != null)   //��׷� Ÿ���� ���� �ϴ� ���
        {
            if(MonState == AnimState.attack) //���� ���� �϶�
            {
                if(0.0001f < m_MoveDir.magnitude)
                {
                    m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                                        m_TargetRot, Time.deltaTime * m_RotSpeed);
                }

                ChangeAnimState(AnimState.attack, 0.12f);   //���� �ִϸ��̼� ����
            }
            else if(MonState == AnimState.trace)   //�������� �϶� 
            {
                if (0.0001f < m_MoveDir.magnitude)
                {
                    m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                                        m_TargetRot, Time.deltaTime * m_RotSpeed);
                }

                //���� ���� �ִϸ��̼� ���̸� ���� �ִϰ� ���� ��쿡�� ���� �̵��ϵ���...
                if (IsAttackAnim() == true)
                    return;

                //--- ���� �̵� �ڵ�
                m_MoveNextStep = m_MoveDir * (m_MoveVelocity * Time.deltaTime);
                m_MoveNextStep.y = 0.0f;
                transform.position += m_MoveNextStep;

                ChangeAnimState(AnimState.trace, 0.12f);  //���� �ִϸ��̼� ����
            }
            else if(MonState == AnimState.idle)
            {
                ChangeAnimState(AnimState.idle, 0.12f);    //������ �ִϸ��̼� ����
            }
        }//else //if(m_AggroTarget != null)   //��׷� Ÿ���� ���� �ϴ� ���
    }//void MonActionUpdate()

    //�ִϸ��̼� ���� ���� �޼���
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

    //--- ���� �ִ� ���� ����
    float m_CacRate = 0.0f;
    float m_NormalTime = 0.0f;
    //--- ���� �ִ� ���� ����
    bool IsAttackAnim()  //���� �ִϸ��̼� ���� üũ �Լ�
    {
      
        if (m_RefAnimation != null)
        {
            if (m_RefAnimation.IsPlaying(anim.Attack1.name) == true)
            {
                m_NormalTime = m_RefAnimation[anim.Attack1.name].time
                                / m_RefAnimation[anim.Attack1.name].length;

               

                //�Ҽ��� �ѵ����� ������ ���� �Ǿ����� ��� ����
                m_CacRate = m_NormalTime - (int)m_NormalTime;

                //���� �ִϸ��̼� ���κ��� �ƴ� �� (���ݾִϸ޽ó� ���̶�� ��)
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

    public void Event_AttHit() //�ִϸ��̼� �̺�Ʈ �Լ��� ȣ��
    {
        if(m_AggroTarget == null)
            return;

        Vector3 a_DistVec = m_AggroTarget.transform.position - transform.position;
        float a_CacLen = a_DistVec.magnitude;
        a_DistVec.y = 0.0f;

        //���ݰ��� �ȿ� �ִ� ���
        if (Vector3.Dot(transform.forward, a_DistVec.normalized) < 0.0f)
            return; //90���� �Ѵ� ������ �ִٴ� ��

        //���� ���� �ۿ� �ִ� ���
        if ((m_AttackDist + 1.7f) < a_CacLen)
            return;

        m_AggroTarget.GetComponent<Hero_Ctrl>().TakeDamage(10);

    }//public void Event_AttHit() //�ִϸ��̼� �̺�Ʈ �Լ��� ȣ��


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
