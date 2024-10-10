using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Hero_Ctrl : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector] public PhotonView pv = null;

    //--- Hp �� ǥ��
    [HideInInspector] public float CurHp = 1000;
    [HideInInspector] public float MaxHp = 1000;
    float NetHp = 1000;  //CurHp �߰��
    public Image ImgHpbar;
    //--- Hp �� ǥ��

    //--- Ű���� �̵� ���� ���� ����
    float h = 0, v = 0;

    float m_CacRotY = 0.0f;
    float m_RotKbSpeed = 150.0f;    //�ʴ� 150�� ȸ���Ϸ��� �ӵ�
    //--- Ű���� �̵� ���� ���� ����

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

    //--- ��ġ ������ �ۼ����� �� ����� ���� ���� �� �ʱⰪ ����
    Vector3 CurPos = Vector3.zero;
    Quaternion CurRot = Quaternion.identity;
    //--- ��ġ ������ �ۼ����� �� ����� ���� ���� �� �ʱⰪ ����

    //--- Animator ���� ����
    Animator m_Animator = null;
    AnimState m_PreState = AnimState.idle;
    AnimState m_CurState = AnimState.idle;
    //--- Animator ���� ����

    //--- ���� ���� ����
    GameObject[] m_EnemyList = null;    //�ʵ���� ���͵��� �������� ���� ����

    float m_AttackDist = 1.9f;          //���ΰ��� ���ݰŸ�
    GameObject m_TargetEnemy = null;    //���� ��� ���� ��ü ���� ����

    Vector3 m_CacTgVec = Vector3.zero;  //Ÿ�ϱ����� �Ÿ� ���� ����
    Vector3 m_CacAtDir = Vector3.zero;  //���ݽ� ���� ��ȯ�� ����
    //--- ���� ���� ����

    public Text NickName;

    void Awake()
    {
        //PhotonView ������Ʈ ã�Ƴ���
        pv = GetComponent<PhotonView>();

        if (pv.IsMine == true)
        {
            Camera_Ctrl a_CamCtrl = Camera.main.GetComponent<Camera_Ctrl>();
            if (a_CamCtrl != null)
                a_CamCtrl.InitCamera(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (pv.IsMine == true)
            GameMgr.Inst.m_RefHero = this;

        m_Animator = this.GetComponent<Animator>();

        LayerMask = 1 << LayerMask.NameToLayer("MyTerrain");
        LayerMask |= 1 << LayerMask.NameToLayer("MyUnit");  //Unit(Enemy)�� ��ŷ

        //���� Hero�� ��ġ �� ȸ�� ���� ó���� ������ �ʱⰪ ����
        CurPos = transform.position;
        CurRot = transform.rotation;
        //���� Hero�� ��ġ �� ȸ�� ���� ó���� ������ �ʱⰪ ����

        NickName.text = pv.Owner.NickName;
    }

    // Update is called once per frame
    void Update()
    {
        //������ Ÿ�ֿ̹� ���� �������� ���ÿ��� ���� �������
        //LoadScene("LobbyScene")�� ���÷��� �ʰ� ȣ��Ǵ� ���� �ذ��
        if (PhotonNetwork.CurrentRoom == null ||
            PhotonNetwork.LocalPlayer == null)
            return;  //����ȭ ������ ������ ���� ������Ʈ�� ����� �ش�.

        if (pv.IsMine == true)
        {
            MousePickCheck();

            EnemyMonitor();

            KeyBDMove();
            JoyStickMvUpdate();
            MousePickUpdate();

            AttackRotUpdate();

            UpdateAnimState();
        }//if (pv.IsMine == true)
        else  //������ �ƹ�Ÿ Pc ���� ��ġ, ȸ��, �ִϸ��̼��� ������� ����ȭ ó��
        {
            if (10.0f < (transform.position - CurPos).magnitude)
            {
                transform.position = CurPos;
            }
            else
            {
                //���� �÷��̾��� �÷��̾ ���� ���� ��ġ���� �ε巴�� �̵���Ŵ
                transform.position = Vector3.Lerp(transform.position, CurPos, Time.deltaTime * 10.0f);
            }

            //���� �÷��̾��� �÷��̾ ���� ���� ������ŭ �ε巴�� ȸ����Ŵ
            transform.rotation = Quaternion.Slerp(transform.rotation, CurRot, Time.deltaTime * 10.0f);

            //--- Remote Animation
            ChangeAnimState(m_CurState); //������ �ƹ�Ÿ���� ���⼭ �ִϸ��̼� ����ȭ

            Remote_TakeDamage();  //������ �ƹ�Ÿ���� ���⼭ Hp ����ȭ�� ���󰣴�.

        }//else  //������ �ƹ�Ÿ Pc ���� ��ġ, ȸ��, �ִϸ��̼��� ������� ����ȭ ó��

    }//void Update()

    void KeyBDMove()
    {
        h = Input.GetAxisRaw("Horizontal"); //ȭ��ǥŰ �¿�Ű�� ������ -1.0f ~ 1.0f
        v = Input.GetAxisRaw("Vertical");

        if (0.0f != h || 0.0f != v)
        {
            m_TargetEnemy = null;        //�켱 Ÿ�� ��ȿȭ

            if (IsSkill() == true)
                return;

            ClearMsPickMove();

            m_CacRotY = transform.eulerAngles.y;
            m_CacRotY += h * m_RotKbSpeed * Time.deltaTime;
            transform.eulerAngles = new Vector3(0.0f, m_CacRotY, 0.0f);

            Vector3 MoveVec = transform.forward * Mathf.Clamp(v, 0.0f, 1.0f);
            transform.position += MoveVec.normalized * m_MoveVelocity * Time.deltaTime;
            ChangeAnimState(AnimState.move);
        }
    }//void KeyBDMove()

    public void SetJoyStickMv(float a_JoyMvLen, Vector3 a_JoyMvDir)
    {
        m_JoyMvLen = a_JoyMvLen;
        if (0.0f < a_JoyMvLen)
        {
            //���콺 ��ŷ �̵� ���
            ClearMsPickMove();

            //m_JoyMvDir.x = a_JoyMvDir.x;
            //m_JoyMvDir.y = 0.0f;
            //m_JoyMvDir.z = a_JoyMvDir.y;
            //m_JoyMvDir.Normalize();

            //--- ī�޶� �ٶ󺸰� �ִ� ������ �������� ȸ�� �����ִ� �ڵ�
            Vector3 a_CamFwVec = Camera.main.transform.forward;
            a_CamFwVec.y = 0.0f;
            a_CamFwVec.Normalize();
            m_JoyMvDir = a_CamFwVec * a_JoyMvDir.y;
            //�� �Ʒ� ����(ī�޶� �ٶ󺸰� �ִ� �������� ��, ��(Z����)�� ��ŭ �̵���ų ����?
            Vector3 a_CamRtVec = Camera.main.transform.right;
            m_JoyMvDir += a_CamRtVec * a_JoyMvDir.x;
            //�¿� ����(ī�޶� �ٶ󺸰� �ִ� �������� ��, ��(X����)�� ��ŭ �̵���ų ����?
            m_JoyMvDir.y = 0.0f;
            m_JoyMvDir.Normalize();
            //--- ī�޶� �ٶ󺸰� �ִ� ������ �������� ȸ�� �����ִ� �ڵ�

        }//if(0.0f < a_JoyMvLen)
    }

    void JoyStickMvUpdate()
    {
        if (0.0f != h || 0.0f != v)
            return;

        //--- ���̽�ƽ �̵� ó��
        if (0.0f < m_JoyMvLen)
        {
            m_TargetEnemy = null;        //�켱 Ÿ�� ��ȿȭ

            if (IsSkill() == true)
                return;

            m_MoveDir = m_JoyMvDir;

            //--- ĳ���� ȸ��
            if (0.0001f < m_JoyMvDir.magnitude)
            {
                m_TargetRot = Quaternion.LookRotation(m_JoyMvDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                        m_TargetRot, Time.deltaTime * m_RotSpeed);
            }
            //--- ĳ���� ȸ��

            transform.position += m_JoyMvDir * (m_MoveVelocity * Time.deltaTime);
            ChangeAnimState(AnimState.move);

        }//if(0.0f < m_JoyMvLen)
    }//void JoyStickMvUpdate()

    void MousePickCheck()   //���콺 Ŭ�� ������ ���� �Լ�
    {
        if (Input.GetMouseButtonDown(0) == true) //���� ���콺 ��ư Ŭ����
            if (GameMgr.IsPointerOverUIObject() == false)    //UI�� �ƴ� ���� Ŭ������ ���� ��ŷ �̵� ���
            {
                if (IsSkill() == true)  //��ų �ߵ� ���� ���� ���콺 Ŭ�� �̵� ����
                    return;

                m_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(m_MousePos, out hitInfo, Mathf.Infinity, LayerMask.value))
                {
                    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("MyUnit"))
                    {  //���콺�� ���͸� ��ŷ �ߴٸ�...
                        MousePicking(hitInfo.point, hitInfo.collider.gameObject);

                        if (GameMgr.Inst.m_CursorMark != null)
                            GameMgr.Inst.m_CursorMark.SetActive(false);
                    }
                    else  //���� �ٴ� ��ŷ�� ��
                    {
                        MousePicking(hitInfo.point);
                        GameMgr.Inst.CursorMarkOn(hitInfo.point);
                    }
                }//if(Physics.Raycast(m_MousePos, out hitInfo, Mathf.Infinity, LayerMask.value))

            }//if(Input.GetMouseButtonDown(0) == true) //���� ���콺 ��ư Ŭ����
    }//void MousePickCheck()   //���콺 Ŭ�� ������ ���� �Լ�

    void MousePicking(Vector3 a_PickVec, GameObject a_PickMon = null) //���콺 Ŭ�� ó�� �Լ�
    {
        a_PickVec.y = transform.position.y;     //��ǥ ��ġ
        //Vector3 a_StartPos = transform.position;    //��� ��ġ

        m_CacLenVec = a_PickVec - transform.position;
        m_CacLenVec.y = 0.0f;

        //--- Picking Enemy ���� ó�� �κ�
        if (a_PickMon != null)
        {
            //���� �����Ϸ��� �ϴ� ������ ��׷� Ÿ���� �����...
            //���� ���ðŸ�... Ÿ���� �ְ�, +1.0�̸� ������ ���͵� �ٰ��ò���
            //�� ���� ���� �ִϸ��̼ǿ� ���� ��ö� move �ִϰ� ���� ���� ���Ѵ�.
            float a_AttDist = m_AttackDist;
            if (a_PickMon.GetComponent<MonsterCtrl>().m_AggroTarget == this.gameObject)
            {
                a_AttDist = m_AttackDist + 1.0f;
            }

            m_CacTgVec = a_PickMon.transform.position - transform.position;

            if (m_CacTgVec.magnitude <= a_AttDist)
            {
                m_TargetEnemy = a_PickMon;
                AttackOrder();  //��ð���

                return;
            }
        }//if(a_PickMon != null)
        //--- Picking Enemy ���� ó�� �κ�

        if (m_CacLenVec.magnitude < 0.5f)   //�ʹ� �ٰŸ� ��ŷ�� ��ŵ�� �ش�.
            return;

        m_TargetPos = a_PickVec;    //���� ��ǥ ��ġ
        m_IsPickMoveOnOff = true;   //��ŷ �̵� OnOff

        m_MoveDir = m_CacLenVec.normalized;
        m_MoveDurTime = m_CacLenVec.magnitude / m_MoveVelocity; //�����ϴµ����� �ɸ��� �ð�
        m_AddTimeCount = 0.0;

        m_TargetEnemy = a_PickMon;
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
                ChangeAnimState(AnimState.move);
            }

            //--- Ÿ���� ���� ��ŷ �̵� ����
            if (m_TargetEnemy != null)
            {
                m_CacTgVec = m_TargetEnemy.transform.position - transform.position;
                if (m_CacTgVec.magnitude <= m_AttackDist) //���ݰŸ�
                    AttackOrder();
            }
            //--- Ÿ���� ���� ��ŷ �̵� ����

        }//if (m_IsPickMoveOnOff == true)

    }//void MousePickUpdate()  //���콺 Ŭ������ ĳ���� �̵��� ����ϴ� �Լ�

    void ClearMsPickMove()
    {
        m_IsPickMoveOnOff = false; //���콺 Ŭ�� �̵� ���

        //���콺 Ŭ�� ���
        if (GameMgr.Inst.m_CursorMark != null)
            GameMgr.Inst.m_CursorMark.SetActive(false);

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

        //if (pv.IsMine == true)
        //{

        m_PreState = newState;
        m_CurState = newState;
    }

    //�ִϸ��̼� ���� ������Ʈ �޼���
    void UpdateAnimState()
    {
        if (IsAttack() == true)
            return;

        //Ű����, ���̽�ƽ, ��ŷ �̵����� �ƴ� ���� ���̵� �������� ���ư��� �Ѵ�.
        if ((0.0f == h && 0.0f == v) && m_JoyMvLen <= 0.0f
            && m_IsPickMoveOnOff == false && IsAttack() == false)
        {
            ChangeAnimState(AnimState.idle);
        }
    }

    //���� ���� ������ Ȯ���ϴ� �޼���
    public bool IsAttack()
    {
        return m_CurState == AnimState.attack || m_CurState == AnimState.skill;
    }

    //���� ��ų �ִϸ��̼� ������ Ȯ���ϴ� �޼���
    public bool IsSkill()
    {
        return m_CurState == AnimState.skill;
    }


    //���� ��� ó�� �޼���
    public void AttackOrder()
    {
        if (pv.IsMine == false)  //IsMine �ƴϸ� ���� ���� ����
            return;

        if (IsAttack() == false) //�������̰ų� ��ų ������� �ƴҶ���...
        {
            //Ű���� ��Ʈ���̳� ���̽�ƽ ��Ʈ�ѷ� �̵� ���̰�
            //����Ű�� ��Ÿ�ؼ� ������ �޸��� �ִϸ��̼ǿ� ��񵿾�
            //���� �ִϰ� ������ ������ �߻��Ѵ�. <--- �̷� ���� ���� ����ó��
            if ((0.0f != h || 0.0f != v) || 0.0f < m_JoyMvLen)
                return;

            ChangeAnimState(AnimState.attack);
            ClearMsPickMove();
        }
    }//public void AttackOrder()

    //��ų ��� ó�� �޼���
    public void SkillOrder(string Type, ref float CoolDur, ref float CurCool)
    {
        if (pv.IsMine == false)  //IsMine �ƴϸ� ���� ���� ����
            return;

        if (0.0f < CurCool)
            return;

        if (m_PreState == AnimState.skill)
            return;

        ChangeAnimState(AnimState.skill);

        ClearMsPickMove();

        CoolDur = 7.0f;   //CoolDuration
        CurCool = CoolDur;
    }

    #region --- �̺�Ʈ �Լ�

    void Event_AttHit()
    {
        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");
        int a_iCount = m_EnemyList.Length;
        float a_fCacLen = 0.0f;
        GameObject a_EffObj = null;
        Vector3 a_EffPos = Vector3.zero;
        //--- �ֺ� ��� ���͸� ã�Ƽ� �������� �ش�.(���� ����)
        for (int i = 0; i < a_iCount; i++)
        {
            m_CacTgVec = m_EnemyList[i].transform.position - transform.position;
            a_fCacLen = m_CacTgVec.magnitude;
            m_CacTgVec.y = 0.0f;

            //���ݰ��� �ȿ� �ִ� ���
            //45�� ���� ���� �ۿ� �ִٸ� ��
            if (Vector3.Dot(transform.forward, m_CacTgVec.normalized) < 0.45f)
                continue;

            //���� �Ÿ� �ۿ� �ִ� ���
            if (m_AttackDist + 0.1f < a_fCacLen)
                continue;

            //����Ʈ ����
            a_EffObj = EffPool.Inst.GetEffObj("Fx_Hit_01", Vector3.zero, Quaternion.identity);
            a_EffPos = m_EnemyList[i].transform.position;
            a_EffPos.y += 1.1f;
            a_EffObj.transform.position = a_EffPos + (-m_CacTgVec.normalized * 1.13f);
            a_EffObj.transform.LookAt(a_EffPos + (m_CacTgVec.normalized * 2.0f));

            m_EnemyList[i].GetComponent<MonsterCtrl>().TakeDamage(this.gameObject);
        }//for(int i = 0; i < a_iCount; i++)
    }//void Event_AttHit()

    //���� ���� �̺�Ʈ ó�� �޼���
    void Event_AttFinish()
    {
        if (pv.IsMine == false)  //���� �ִϸ��̼��� ������ ���� �ൿ�� ���� �Ǵܵ� IsMine ��
            return;

        if ((0.0f != h || 0.0f != v) || 0.0f < m_JoyMvLen || m_IsPickMoveOnOff == true)
        //Ű���� �̵������� �ְų�, ���̽�ƽ ������ �ִ� ���
        {   //��ų ��� �߿� �̵��� ������ �ִٰ� �������� ������ 
            //���� ��� ��Ҹ� ���� �ڵ�
            ChangeAnimState(AnimState.move);
            //m_RsvPicking = 0.0f;
            return;
        }

        ////Attack�����ϴ�� Attack���·� ������ �Ѵ�.
        if (m_CurState != AnimState.skill) return; //why? -- 

        if (IsTargetEnemyActive(0.2f) == true)
        {
            ChangeAnimState(AnimState.attack);
            ClearMsPickMove();
        }
        else
        {
            ChangeAnimState(AnimState.idle);
        }
    }

    void Event_SkillHit()
    {
        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");
        int a_iCount = m_EnemyList.Length;
        float a_fCacLen = 0.0f;
        GameObject a_EffObj = null;
        Vector3 a_EffPos = Vector3.zero;

        a_EffObj = EffPool.Inst.GetEffObj("Fx_AttackCritical_01", Vector3.zero, Quaternion.identity);

        a_EffPos = transform.position;
        a_EffPos.y += 1.1f;
        a_EffObj.transform.position = a_EffPos + (transform.forward * 2.3f);
        a_EffObj.transform.LookAt(a_EffPos + (-transform.forward * 2.0f));


        for (int i = 0; i < a_iCount; i++)
        {
            m_CacTgVec = m_EnemyList[i].transform.position - transform.position;
            a_fCacLen = m_CacTgVec.magnitude;
            m_CacTgVec.y = 0.0f;

            //���� ���� ���� ���� 360�� ��� ������ �ֱ�

            //���� ���� �ۿ� �ִ� ���
            if (m_AttackDist + 0.1f < a_fCacLen)
                continue;


            a_EffObj = EffPool.Inst.GetEffObj("Fx_Hit_01", Vector3.zero, Quaternion.identity);
            a_EffPos = m_EnemyList[i].transform.position;
            a_EffPos.y += 1.1f;
            a_EffObj.transform.position = a_EffPos + (-m_CacTgVec.normalized * 1.13f);
            a_EffObj.transform.LookAt(a_EffPos + (m_CacTgVec.normalized * 2.0f));



            m_EnemyList[i].GetComponent<MonsterCtrl>().TakeDamage(this.gameObject, 50);
        }//for(int i = 0; i < a_iCount; i++)
    }//void Event_SkillHit()

    // ��ų ���� �̺�Ʈ ó�� �޼���
    void Event_SkillFinish()
    {
        if (pv.IsMine == false)  //���� �ִϸ��̼��� ������ ���� �ൿ�� ���� �Ǵܵ� IsMine ��
            return;

        if ((0.0f != h || 0.0f != v) || 0.0f < m_JoyMvLen || m_IsPickMoveOnOff == true)
        //Ű���� �̵������� �ְų�, ���̽�ƽ ������ �ִ� ���
        {   //��ų ��� �߿� �̵��� ������ �ִٰ� �������� ������ 
            //���� ��� ��Ҹ� ���� �ڵ�
            ChangeAnimState(AnimState.move);
            //m_RsvPicking = 0.0f;
            return;
        }

        //Skill �����ε� Attack�ִϸ��̼� ���� ���� ����� ���ܽ��ѹ�����.
        //���� �ִ� �߿� ��ų �ߵ��� ���� ������ �̺�Ʈ �Լ��� ���ͼ� ��ų��
        //��ҵǴ� ������ ���� �� �־ ���� ó����
        //Skill�����϶��� Skill���·� ������ �Ѵ�.
        if (m_CurState != AnimState.skill)
            return;

        if (IsTargetEnemyActive(0.2f) == true)
        {
            ChangeAnimState(AnimState.attack);
            ClearMsPickMove();
        }
        else
        {
            ChangeAnimState(AnimState.idle);
        }
    }

    #endregion

    // Ÿ�� ���� ��ȿ���� Ȯ���ϴ� �Լ�
    bool IsTargetEnemyActive(float a_ExtLen = 0.0f)
    {
        if (m_TargetEnemy == null)
            return false;

        // Ÿ���� Ȱ��ȭ�Ǿ� ���� ������ Ÿ�� ����
        if (m_TargetEnemy.activeSelf == false)
        {
            m_TargetEnemy = null;
            return false;
        }

        //isDie �׾� �־
        MonsterCtrl a_Unit = m_TargetEnemy.GetComponent<MonsterCtrl>();
        if (a_Unit.m_CurState == AnimState.die)  //�׾�����...
        {
            m_TargetEnemy = null;
            return false;
        }

        m_CacTgVec = m_TargetEnemy.transform.position - transform.position;
        m_CacTgVec.y = 0.0f;
        if (m_AttackDist + a_ExtLen < m_CacTgVec.magnitude)
        { //(���ݰŸ� + �˻� Ȯ��Ÿ�) �ٱ��ʿ� ���� ��쵵 Ÿ���� ��ȿȭ �� ������. 
            //m_TargetEnemy = null; //���Ÿ��� ��� Ÿ���� ������ �� �����ϱ�...
            return false;
        }

        return true; //Ÿ���� ���� ��ȿ �ϴٴ� �ǹ�
    }

    void EnemyMonitor()
    {
        //���콺 ��ŷ�� �õ��߰� �̵� ���̸� Ÿ���� �ٽ� ���� �ʴ´�.
        if (m_IsPickMoveOnOff == true)
            return;

        //���� ������ ��Ȯ�� ������ ���� �ִϸ��̼Ǹ� �ϰ� ���� ����...
        if (IsAttack() == false)    //���� �ִϸ��̼��� �ƴϸ�...
            return;

        //���� �ִϸ��̼� ���̰� Ÿ���� ��ȿȭ �Ǿ��ٸ�... Ÿ���� ���� ����ش�.
        //Ÿ���� ��ü�� ���ݰŸ����ٴ� ���� �� ����(0.5f)�� �ΰ� �ٲٰ� �Ѵ�.
        if (IsTargetEnemyActive(0.5f) == false)
            FindEnemyTarget();
    }

    void FindEnemyTarget()
    {
        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");

        float a_MinLen = float.MaxValue;
        int a_iCount = m_EnemyList.Length;
        m_TargetEnemy = null;      //�켱 Ÿ�� ��ȿȭ
        for (int i = 0; i < a_iCount; i++)
        {
            m_CacTgVec = m_EnemyList[i].transform.position - transform.position;
            m_CacTgVec.y = 0.0f;
            if (m_CacTgVec.magnitude <= m_AttackDist)
            {   //���ݰŸ� ���ʿ� ���� ��츸 Ÿ������ ��´�.

                if (m_CacTgVec.magnitude < a_MinLen)
                {
                    a_MinLen = m_CacTgVec.magnitude;
                    m_TargetEnemy = m_EnemyList[i];
                }//if(m_CacTgVec.magnitude < a_MinLen)
            }//if(m_CacTgVec.magnitude <= m_AttackDist)
        }//for(int i = 0; i < a_iCount; i++)

    }//void FindEnemyTarget()

    float m_CacRotSpeed = 0.0f;
    public void AttackRotUpdate()
    {   //���ݾִϸ��̼� ���� �� Ÿ���� ���� ȸ���ϰ� �ϴ� �Լ�

        if (m_TargetEnemy == null) //Ÿ���� �������� ������...
            return;

        //if (IsAttack() == false)    //���� �ִϸ��̼��� �ƴϸ�...
        //    return;

        m_CacTgVec = m_TargetEnemy.transform.position - transform.position;
        m_CacTgVec.y = 0.0f;

        if (m_CacTgVec.magnitude <= (m_AttackDist + 0.3f))   //���ݰŸ�
        {
            m_CacAtDir = m_CacTgVec.normalized;
            if (0.0001f < m_CacAtDir.magnitude)
            {
                m_CacRotSpeed = m_RotSpeed * 3.0f;  //�ʴ� ȸ�� �ӵ�
                Quaternion a_TargetRot = Quaternion.LookRotation(m_CacAtDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                a_TargetRot,
                                Time.deltaTime * m_CacRotSpeed);
            }//if(0.0001f < m_CacAtDir.magnitude)
        }//if(m_CacTgVec.magnitude <= (m_AttackDist + 0.3f))   //���ݰŸ�
    }//public void AttackRotUpdate()

    public void TakeDamage(float a_Damage = 10.0f)
    {
        if (CurHp <= 0.0f)
            return;

        if (pv.IsMine == true)
        { //���� ���̴̹� IsMine �� ���� ����ش�. IsMine�� �ƴϸ� ���⸸...
            CurHp -= a_Damage;
            if (CurHp < 0.0f)
                CurHp = 0.0f;

            ImgHpbar.fillAmount = CurHp / MaxHp;
        }//if (pv.IsMine == true)

        //Vector3 a_CacPos = this.transform.position;
        //a_CacPos.y += 2.65f;
        //GameMgr.Inst.SpawnDamageText((int)a_Damage, a_CacPos, 1);

        if (pv.IsMine == true)  //���ó���� IsMine �϶���... �������� ����ȭ�� ó����
            if (CurHp <= 0)  //���ó��
            {
                //Die();
            }
    }//public void TakeDamage(float a_Damage = 10.0f)

    void Remote_TakeDamage() //������ ��ǻ�Ϳ��� Hp ����ȭ �Լ�
    {
        if (0.0f < CurHp)
        {
            CurHp = NetHp;

            //Image UI �׸��� fillAmount �Ӽ��� ������ ���� ������ �� ����
            ImgHpbar.fillAmount = CurHp / (float)MaxHp;

            if (CurHp <= 0)  //���ó�� �ѹ��� ȣ��ǰ� �ϱ� ���Ͽ�...
            {
                //Die();
            }
        }
    }//void Remote_TakeDamge() //������ ��ǻ�Ϳ��� Hp ����ȭ �Լ�

    [PunRPC]
    public void TakeItemRPC(int a_ItemType)
    {
        if (pv.IsMine == false) return;
        if (CurHp <= 0.0f) return;

        CurHp += 30.0f;

        if (MaxHp <= CurHp) CurHp = MaxHp;

        Vector3 a_CacPos = this.transform.position;

        a_CacPos.y += 2.65f;

        GameMgr.Inst.SpawnDamageText(30, a_CacPos, 1);

        ImgHpbar.fillAmount = CurHp / MaxHp;

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //���� �÷��̾��� ��ġ ���� �۽�
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(CurHp);
            stream.SendNext((int)m_CurState);
        }
        else //���� �÷��̾��� ��ġ ���� ����
        {
            CurPos = (Vector3)stream.ReceiveNext();
            CurRot = (Quaternion)stream.ReceiveNext();
            NetHp  = (float)stream.ReceiveNext();
            m_CurState = (AnimState)stream.ReceiveNext();
        }
    }//public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
}
