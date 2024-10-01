using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hero_Ctrl : MonoBehaviour
{
    #region HP
    [HideInInspector] public float m_MaxHP = 1000.0f;
    [HideInInspector] public float m_CurHP = 1000.0f;
    public Image m_HPBar = null;
    #endregion


    #region Ű���� �̵�
    float h = 0, v = 0;

    float m_CacRotY = 0.0f;
    float m_RotKbSpeed = 150.0f;    //�ʴ� 150�� ȸ���Ϸ��� �ӵ�
    #endregion

    float m_MoveVelocity = 5.0f;  //��� �ʴ� �̵� �ӵ�...

    //--- JoyStick �̵� ó�� ����
    float m_JoyMvLen = 0.0f;
    Vector3 m_JoyMvDir = Vector3.zero;
    //--- JoyStick �̵� ó�� ����

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

    //--- Animator ���� ����
    Animator m_Animator = null;
    AnimState m_PreState = AnimState.idle;
    AnimState m_CurState = AnimState.idle;
    //--- Animator ���� ����


    #region ����
    GameObject m_TargetE = null;
    GameObject[] m_EList = null;
    float m_AttackDist = 1.8f;

    Vector3 m_CacTVec = Vector3.zero;
    Vector3 m_CacAttDir = Vector3.zero;
    #endregion


    void Awake()
    {
        Camera_Ctrl a_CamCtrl = Camera.main.GetComponent<Camera_Ctrl>();
        if (a_CamCtrl != null)
            a_CamCtrl.InitCamera(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        //if(pv.IsMine == true)
        GameMgr.Inst.m_RefHero = this;

        m_Animator = this.GetComponent<Animator>();

        LayerMask = 1 << LayerMask.NameToLayer("MyTerrain");
    }

    // Update is called once per frame
    void Update()
    {
        MousePickCheck();

        EnermyMonitor();


        KeyBDMove();
        JoyStickMvUpdate();
        MousePickUpdate();

        AttRotUpdate();

        UpdateAnimState();
    }

    void KeyBDMove()
    {
        h = Input.GetAxisRaw("Horizontal"); //ȭ��ǥŰ �¿�Ű�� ������ -1.0f ~ 1.0f
        v = Input.GetAxisRaw("Vertical");

        if (0.0f != h || 0.0f != v)
        {

            m_TargetE = null;

            if (IsSkill() == true) return;

            ClearMsPickMove();

            m_CacRotY = transform.eulerAngles.y;
            m_CacRotY += h * m_RotKbSpeed * Time.deltaTime;
            transform.eulerAngles = new Vector3(0.0f, m_CacRotY, 0.0f);

            Vector3 MoveVec = transform.forward * Mathf.Clamp(v, 0.0f, 1.0f);
            transform.position += MoveVec.normalized * m_MoveVelocity * Time.deltaTime;
        }
    }//void KeyBDMove()

    public void SetJoyStickMv(float a_JoyMvLen, Vector3 a_JoyMvDir)
    {
        m_JoyMvLen = a_JoyMvLen;
        if (0.0f < a_JoyMvLen)
        {
            // ���콺 �� �̵� Ŭ����
            ClearMsPickMove();

            // ī�޶� ���⿡ ���� �̵� ���� ���
            Vector3 a_CamFwVec = Camera.main.transform.forward;
            a_CamFwVec.y = 0.0f;
            a_CamFwVec.Normalize();
            m_JoyMvDir = a_CamFwVec * a_JoyMvDir.y;

            Vector3 a_CamRtVec = Camera.main.transform.right;
            m_JoyMvDir += a_CamRtVec * a_JoyMvDir.x;
            m_JoyMvDir.y = 0.0f;
            m_JoyMvDir.Normalize();
        }
        else
        {
            m_JoyMvDir = Vector3.zero;
        }
    }

    void JoyStickMvUpdate()
    {
        if (0.0f != h || 0.0f != v)
            return;

        // ���̽�ƽ �̵� ó��
        if (0.0f < m_JoyMvLen)
        {
            m_TargetE = null;

            if (IsSkill() == true) return;

            m_MoveDir = m_JoyMvDir;

            // ĳ���� ȸ��
            if (0.0001f < m_JoyMvDir.magnitude)
            {
                m_TargetRot = Quaternion.LookRotation(m_JoyMvDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                        m_TargetRot, Time.deltaTime * m_RotSpeed);
            }

            transform.position += m_JoyMvDir * (m_MoveVelocity * Time.deltaTime);
        }
    }

    void MousePickCheck()   //���콺 Ŭ�� ������ ���� �Լ�
    {
        if (Input.GetMouseButtonDown(0) == true) //���� ���콺 ��ư Ŭ����
            if (GameMgr.IsPointerOverUIObject() == false)    //UI�� �ƴ� ���� Ŭ������ ���� ��ŷ �̵� ���
            {
                if (IsSkill() == true) return;

                m_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(m_MousePos, out hitInfo, Mathf.Infinity, LayerMask.value))
                {
                    MousePicking(hitInfo.point);
                    GameMgr.Inst.CurMarkOn(hitInfo.point);
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


        m_TargetE = a_PickMon;
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

        if (GameMgr.Inst.m_CursorMark != null)
            GameMgr.Inst.m_CursorMark.SetActive(false);

        //���콺 Ŭ�� ���
    }//void ClearMsPickMove()

    //�ִϸ��̼� ���� ���� �޼���
    public void ChangeAnimState(AnimState newState, float crossTime = 0.1f, string animName = "")
    {
        if (m_Animator == null)
            return;

        if (m_PreState == newState)
            return;

        m_Animator.ResetTrigger(m_PreState.ToString()); //������ ����Ǿ� �ִ� Trigger ������ ����

        if (0.0f < crossTime)
        {
            m_Animator.SetTrigger(newState.ToString());
        }
        else
        {
            m_Animator.Play(animName, -1, 0);
            //��� -1�� Layer Index, �ڿ� 0�� ó������ �ٽ� ���� �÷��� ��Ű�ڴٴ� �ǹ�
        }

        m_PreState = newState;
        m_CurState = newState;
    }

    //�ִϸ��̼� ���� ������Ʈ �޼���
    void UpdateAnimState()
    {
        if (IsAttack() == true) return;



        //��� ���� �̵��� �ƴϸ� idle�� ����
        if ((0 == h&& 0 == v) && 0 == m_JoyMvLen && m_IsPickMoveOnOff == false && !IsAttack())
        {
            ChangeAnimState(AnimState.idle);
        }
        //������ �޸����
        else
        {
            ChangeAnimState(AnimState.move);
        }
    }

    public void Attack()
    {
        if (!IsAttack())
        {
            //�̵� ���̸鼭 ����Ű�� ��Ÿ�ؼ� ������ ���� �ִϰ� �����.
            //������ �ڵ�
            if ((0 != h || 0 != v) || 0 < m_JoyMvLen) return;

            ChangeAnimState(AnimState.attack);
            ClearMsPickMove();
        }
    }

    public void Skill(string a_Type, ref float a_CoolDur, ref float a_CurCool)
    {
        if (0 < a_CurCool) return;

        if (m_PreState == AnimState.skill) return;

        ChangeAnimState(AnimState.skill);

        ClearMsPickMove();

        a_CoolDur = 7.0f;
        a_CurCool =  a_CoolDur;
    }

    public bool IsAttack()
    {
        return m_CurState == AnimState.attack || m_CurState == AnimState.skill;
    }

    public bool IsSkill()
    {
        return m_CurState == AnimState.skill;

    }


    void Event_Finish()
    {
        //if (m_CurState == AnimState.skill) return;

        if (IsTargetE_Active(0.2f) == true)
        {
            ChangeAnimState(AnimState.attack);
            ClearMsPickMove();
        }
        else
        {
            ChangeAnimState(AnimState.idle);

        }
    }

    void Event_SkillF()
    {
        //if(m_CurState == AnimState.attack) return;//attack ���¿����� ��ų ��� �Ұ���

        if (IsTargetE_Active(0.2f) == true)
        {
            ChangeAnimState(AnimState.attack);
            ClearMsPickMove();
        }
        else
        {
            ChangeAnimState(AnimState.idle);

        }
    }


    bool IsTargetE_Active(float exLen = 0)
    {
        if (m_TargetE == null)
            return false;
        //Ÿ�� ��Ȱ��ȭ �� false
        if (m_TargetE.activeSelf == false)
            return false;

        Mon_Ctrl a_Unit = m_TargetE.GetComponent<Mon_Ctrl>();
        if (a_Unit.m_CurState == AnimState.die)
        {
            m_TargetE = null;
            return false;
        }

        m_CacTVec = m_TargetE.transform.position - transform.position;
        m_CacTVec.y = 0.0f;

        if (m_AttackDist + exLen < m_CacTVec.magnitude)
        {
            m_TargetE = null;
            return false;
        }


        return true;

    }

    void EnermyMonitor()
    {
        if (m_IsPickMoveOnOff == true) return;

        //������ ������ ���� ���� ó��
        if (IsAttack() == false) return;

        // ���� �ִϸ��̼� �� Ÿ�� ��ȿȭ��
        if (IsTargetE_Active(0.5f) == false)
            FindE_Target();


    }
    void FindE_Target()
    {
        m_EList = GameObject.FindGameObjectsWithTag("Enermy");

        float a_MinLen = float.MaxValue;
        int m_ICnt = m_EList.Length;

        m_TargetE = null;
        for (int i = 0; i < m_ICnt; i++) // ���⼭ a_MinLen ��� m_ICnt�� ����մϴ�.
        {
            m_CacTVec = m_EList[i].transform.position - transform.position;
            m_CacTVec.y = 0.0f;
            if (m_CacTVec.magnitude <= m_AttackDist)
            {
                if (m_CacTVec.magnitude < a_MinLen)
                {
                    a_MinLen = m_CacTVec.magnitude;
                    m_TargetE = m_EList[i];
                }
            }
        }
    }

    float m_CacRotSpeed = 0.0f;
    public void AttRotUpdate()
    {
        if (m_TargetE == null) return;

        m_CacTVec = m_TargetE.transform.position - transform.position;
        m_CacTVec.y = 0.0f;

        if (m_CacTVec.magnitude <= (m_AttackDist + 0.3f))
        {
            m_CacAttDir = m_CacTVec.normalized;
            if (0.0001f < m_CacAttDir.magnitude)
            {
                m_TargetRot = Quaternion.LookRotation(m_CacAttDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                       m_TargetRot, Time.deltaTime * m_RotSpeed);

            }
        }
    }

    #region ������ ó��
    public void TakeDmg(float a_Dmg = 10)
    {
        if (m_CurHP <= 0) return;

        m_CurHP -= a_Dmg;

        if (m_CurHP < 0) m_CurHP = 0;


        m_HPBar.fillAmount = m_CurHP / m_MaxHP;

        Vector3 a_CacPos = this.transform.position;
        a_CacPos.y += 2.65f;
        GameMgr.Inst.SpawnDmgTxt((int)a_Dmg, a_CacPos, 1);

        if (m_CurHP <= 0)
        {
            ChangeAnimState(AnimState.die);
        }
    }
    #endregion


}
