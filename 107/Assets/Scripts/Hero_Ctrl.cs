using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero_Ctrl : MonoBehaviour
{
    //--- Ű���� �̵� ���� ���� ����
    float h = 0, v = 0;

    float m_CacRotY = 0.0f;
    float m_RotKbSpeed = 150.0f;    //�ʴ� 150�� ȸ���Ϸ��� �ӵ�
                                    //--- Ű���� �̵� ���� ���� ����

    float m_MoveVelocity = 5.0f;  //��� �ʴ� �̵� �ӵ�...

    #region JoyStick 
    float m_JoyMvLen = 0.0f;
    Vector3 m_JoyMvDir = Vector3.zero;
    #endregion

    //--- Picking ���� ����
    Ray m_MousePos;
    RaycastHit hitInfo;
    LayerMask LayerMask = -1;

    bool m_IsPickMoveOnOff = false;     //��ŷ �̵� OnOff
    Vector3 m_TargetPos = Vector3.zero; //���� ��ǥ ��ġ
    Vector3 m_MoveDir = Vector3.zero;   //x, z��� ���� ����
    double m_MoveDurTime = 0;           //��ǥ������ �����ϴµ� �ɸ��� �ð�
    double m_AddTimeCount = 0;          //���� �ð� ī��Ʈ
    Vector3 m_CacLenVec = Vector3.zero; //�̵� ���� ����
    Quaternion m_TargetRot = Quaternion.identity; //ȸ�� ���� ����
    float m_RotSpeed = 7.0f;            //�ʴ� 7�� ȸ���Ϸ��� �ӵ�
                                        //--- Picking ���� ����

    //# Animator 
    Animator m_Anim = null;
    AnimState m_PreState = AnimState.idle;
    AnimState m_CurState = AnimState.idle;

    // ���� ���� ����
    int m_AttackCount = 0;
    float m_AttackCooldown = 0.5f;
    float m_LastAttackTime = 0.0f;

    void Awake()
    {
        Camera_Ctrl a_CamCtrl = Camera.main.GetComponent<Camera_Ctrl>();
        if (a_CamCtrl != null)
            a_CamCtrl.InitCamera(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Game_Mgr.Inst.m_Player = this;
        m_Anim = this.GetComponent<Animator>();
        LayerMask = 1 << LayerMask.NameToLayer("MyTerrain");
    }

    // Update is called once per frame
    void Update()
    {
        MousePickCheck();

        KeyBDMove();
        JSMvUpdate();
        MousePickUpdate();

        AnimStateUpdate();
    }

    void KeyBDMove()
    {
        h = Input.GetAxisRaw("Horizontal"); //ȭ��ǥŰ �¿�Ű�� ������ -1.0f ~ 1.0f
        v = Input.GetAxisRaw("Vertical");

        if (0.0f != h || 0.0f != v)
        {
            ClearMsPickMove();

            m_CacRotY = transform.eulerAngles.y;
            m_CacRotY += h * m_RotKbSpeed * Time.deltaTime;
            transform.eulerAngles = new Vector3(0.0f, m_CacRotY, 0.0f);

            Vector3 MoveVec = transform.forward * Mathf.Clamp(v, 0.0f, 1.0f);
            transform.position += MoveVec.normalized * m_MoveVelocity * Time.deltaTime;
        }
    }//void KeyBDMove()

    public void SetJsMv(float a_JoyMvLen, Vector3 a_JoyMvDir)
    {
        m_JoyMvLen = a_JoyMvLen;

        if (0 < a_JoyMvLen)
        {
            ClearMsPickMove();

            //ī�޶� ���� ��������ȸ��
            Vector3 a_CamFVec = Camera.main.transform.forward;
            a_CamFVec.y = 0.0f;
            a_CamFVec.Normalize();
            m_JoyMvDir = a_JoyMvDir.y * a_CamFVec;

            Vector3 a_CamRVec = Camera.main.transform.right;
            m_JoyMvDir += a_JoyMvDir.x * a_CamRVec;
            m_JoyMvDir.Normalize();
            m_JoyMvDir.y = 0.0f;
        }
        else
        {
            m_JoyMvDir = Vector3.zero;
        }
    }

    void JSMvUpdate()
    {
        if (0 != h || 0 != v) return;

        if (0.0f < m_JoyMvLen)
        {
            m_MoveDir = m_JoyMvDir;

            //ĳ���͸� �̵��������� ȸ��
            if (0.0001f < m_JoyMvDir.magnitude)
            {
                m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                       m_TargetRot, Time.deltaTime * m_RotSpeed);
            }

            transform.position += m_JoyMvDir * (m_MoveVelocity * Time.deltaTime);
        }
    }

    void MousePickCheck()   //���콺 Ŭ�� ������ ���� �Լ�
    {
        if (Input.GetMouseButtonDown(0) == true) //���� ���콺 ��ư Ŭ����
            if (Game_Mgr.IsPointerOverUIObject() == false)
            {
                m_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(m_MousePos, out hitInfo, Mathf.Infinity, LayerMask.value))
                {
                    MousePicking(hitInfo.point);
                }
            }//if(Input.GetMouseButtonDown(0) == true) //���� ���콺 ��ư Ŭ����
    }//void MousePickCheck()   //���콺 Ŭ�� ������ ���� �Լ�

    void MousePicking(Vector3 a_PickVec, GameObject a_PickMon = null) //���콺 Ŭ�� ó�� �Լ�
    {
        a_PickVec.y = transform.position.y;     //��ǥ ��ġ
        Vector3 a_StartPos = transform.position;    //��� ��ġ

        m_CacLenVec = a_PickVec - a_StartPos;
        m_CacLenVec.y = 0.0f;

        if (m_CacLenVec.magnitude < 0.5f)   //�ʹ� �ٰŸ� ��ŷ�� ��ŵ�� �ش�.
            return;

        m_TargetPos = a_PickVec;    //���� ��ǥ ��ġ
        m_IsPickMoveOnOff = true;   //��ŷ �̵� OnOff

        m_MoveDir = m_CacLenVec.normalized;
        m_MoveDurTime = m_CacLenVec.magnitude / m_MoveVelocity; //�����ϴµ����� �ɸ��� �ð�
        m_AddTimeCount = 0.0;
    }

    void MousePickUpdate()  //���콺 Ŭ������ ĳ���� �̵��� ����ϴ� �Լ�
    {
        if (m_IsPickMoveOnOff == true)
        {
            m_CacLenVec = m_TargetPos - transform.position;
            m_CacLenVec.y = 0.0f;

            m_MoveDir = m_CacLenVec.normalized;

            //ĳ���͸� �̵��������� ȸ����Ű�� �ڵ�
            if (0.0001f < m_CacLenVec.magnitude)
            {
                m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                    m_TargetRot, Time.deltaTime * m_RotSpeed);
            }
            //ĳ���͸� �̵��������� ȸ����Ű�� �ڵ�

            m_AddTimeCount += Time.deltaTime;
            if (m_MoveDurTime <= m_AddTimeCount) //��ǥ���� ������ ������ �����Ѵ�.
            {
                ClearMsPickMove();
            }
            else
            {
                transform.position += m_MoveDir * Time.deltaTime * m_MoveVelocity;
            }
        }//if (m_IsPickMoveOnOff == true)

    }//void MousePickUpdate()  //���콺 Ŭ������ ĳ���� �̵��� ����ϴ� �Լ�

    void ClearMsPickMove()
    {
        m_IsPickMoveOnOff = false; //���콺 Ŭ�� �̵� ���

        //���콺 Ŭ�� ���
    }//void ClearMsPickMove()

    #region �ִϸ��̼�
    public void ChangeAnimState(AnimState a_State, float a_CrTime = 0.1f, string a_AnimName = "")
    {
        if (m_Anim == null) return;

        if (m_PreState == a_State) return;

        m_Anim.ResetTrigger(m_PreState.ToString());

        m_Anim.SetTrigger(a_State.ToString());

        if (0 <a_CrTime)
        {
            m_Anim.SetTrigger(a_State.ToString());
        }

        else
        {
            m_Anim.Play(a_AnimName, -1, 0);
        }

        m_PreState = a_State;
        m_CurState = a_State;
    }

    public void Attack()
    {
        if (Time.time - m_LastAttackTime > m_AttackCooldown)
        {
            m_AttackCount = 0;
        }

        m_AttackCount++;
        m_LastAttackTime = Time.time;

        if (m_Anim != null)
        {
            AnimatorStateInfo stateInfo = m_Anim.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName("Idle") || stateInfo.IsName("Attack_03"))
            {
                m_Anim.SetTrigger("attack");
            }
            else if (stateInfo.IsName("Attack_02"))
            {
                m_Anim.SetTrigger("attack");
            }
        }
    }

    void AnimStateUpdate()
    {
        AnimatorStateInfo stateInfo = m_Anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Attack_03") && stateInfo.normalizedTime >= 1.0f)
        {
            ChangeAnimState(AnimState.idle);
        }
        else if ((h != 0.0f) || (v != 0.0f) || 0 < m_JoyMvLen || m_IsPickMoveOnOff == true)
        {
            ChangeAnimState(AnimState.move);
        }
        else
        {
            ChangeAnimState(AnimState.idle);
        }
    }

    #endregion
}
