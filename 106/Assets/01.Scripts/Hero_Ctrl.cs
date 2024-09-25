using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // ����ȭ
public class Anim
{
    public AnimationClip Idle;
    public AnimationClip Walk;
}

public class Hero_Ctrl : MonoBehaviour
{
    #region Ű���� �̵�
    float h, v = 0;

    float m_CacRot = 0;
    float m_RotKBSpeed = 150f;
    float m_MoveVel = 5.0f;
    #endregion

    #region ���콺 �̵�
    Ray m_MousePos;
    RaycastHit m_Hit;
    LayerMask m_LayerMask = -1;

    bool m_IsPick = false;
    Vector3 m_TargetPos = Vector3.zero;
    Vector3 m_MoveDir = Vector3.zero;
    double m_MoveDur = 0;
    double m_AddTimeCnt = 0;
    Vector3 m_CacLenVec = Vector3.zero;
    Quaternion m_TargetRot = Quaternion.identity;
    float m_RotSpeed = 7.0f;
    #endregion

    #region ���̽�ƽ �̵�
    float m_JoyMvLen = 0.0f;
    Vector3 m_JoyMvDir = Vector3.zero;
    Vector3 m_JSDirVec = Vector3.zero;
    float m_JSMvSpeed = 5.0f;
    #endregion

    #region �ִϸ��̼�
    // �ִϸ��̼� Ŭ���� ����
    public Anim anim;

    // 3D ���� �ִϸ��̼� ������Ʈ ����
    public Animation _animation;
    private string currentAnimation;
    #endregion

    void Awake()
    {
        Cam_Ctrl a_CamCtrl = Camera.main.GetComponent<Cam_Ctrl>();

        if (a_CamCtrl != null)
            a_CamCtrl.InitCam(gameObject);
    }

    void Start()
    {
        m_LayerMask = 1 << LayerMask.NameToLayer("Test_Terrain");

        // �ִϸ��̼� ������Ʈ ã�� �Ҵ�
        _animation = GetComponentInChildren<Animation>();

        // �ִϸ��̼� ������Ʈ ���� Ŭ�� ����
        _animation.clip = anim.Idle;
        _animation.Play();
        currentAnimation = anim.Idle.name;
    }

    void Update()
    {
        MousePickCheck();
        JoyStickMvUpdate(); // ���̽�ƽ �̵�
        KeyMove();
        MouseUpdate();
    }

    #region ���콺�θ� �̵�
    void MousePickCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(m_MousePos, out m_Hit, Mathf.Infinity, m_LayerMask.value))
            {
                MousePick(m_Hit.point);
            }
        }
    }

    void MousePick(Vector3 a_PosVec, GameObject a_PickMon = null)
    {
        a_PosVec.y = transform.position.y;

        Vector3 a_StartPos = transform.position;

        Vector3 m_CacLenVec = a_PosVec - a_StartPos;
        m_CacLenVec.y = 0;

        if (m_CacLenVec.magnitude < 0.5f) return;

        m_TargetPos = a_PosVec;
        m_IsPick = true;

        m_MoveDir = m_CacLenVec.normalized;

        m_MoveDur = m_CacLenVec.magnitude / m_MoveVel;
        m_AddTimeCnt = 0;
    }

    void MouseUpdate()
    {
        if (m_IsPick == true)
        {
            m_CacLenVec = m_TargetPos - transform.position;
            m_CacLenVec.y = 0;

            m_MoveDir = m_CacLenVec.normalized;

            if (0.0001f < m_CacLenVec.magnitude)
            {
                m_TargetRot = Quaternion.LookRotation(m_MoveDir);

                transform.rotation = Quaternion.Slerp(transform.rotation,
                    m_TargetRot, Time.deltaTime * m_RotSpeed);
            }

            m_AddTimeCnt += Time.deltaTime;

            if (m_MoveDur <= m_AddTimeCnt)
            {
                ClearMsPick();
            }
            else
            {
                transform.position += m_MoveDir * m_MoveVel * Time.deltaTime;
            }

            // �ִϸ��̼� ��ȯ
            if (currentAnimation != anim.Walk.name)
            {
                _animation.CrossFade(anim.Walk.name, 0.3f);
                currentAnimation = anim.Walk.name;
            }
        }
        else
        {
            // �ִϸ��̼� ��ȯ
            if (currentAnimation != anim.Idle.name)
            {
                _animation.CrossFade(anim.Idle.name, 0.3f);
                currentAnimation = anim.Idle.name;
            }
        }
    }

    void ClearMsPick()
    {
        m_IsPick = false;
    }
    #endregion

    #region ���̽�ƽ �̵�
    public void SetJoyStickMv(float a_JoyMvLen, Vector3 a_JoyMvDir)
    {
        m_JoyMvLen = a_JoyMvLen;
        if (0.0f < a_JoyMvLen)
        {
            m_JoyMvDir = new Vector3(a_JoyMvDir.x, 0.0f, a_JoyMvDir.y);
        }
    }

    public void JoyStickMvUpdate()
    {
        if (h != 0.0f || v != 0.0f)
            return;

        // ���̽�ƽ �̵� �ڵ�
        if (0.0f < m_JoyMvLen)
        {
            m_JSDirVec = m_JoyMvDir;
            float a_MvStep = m_JSMvSpeed * Time.deltaTime;
            transform.Translate(m_JoyMvDir * m_JoyMvLen * a_MvStep, Space.World);

            // ĳ���Ͱ� ���̽�ƽ ������ �ٶ󺸵��� ȸ��
            if (m_JoyMvDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(m_JoyMvDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_RotSpeed);
            }

            // �ִϸ��̼� ��ȯ
            if (currentAnimation != anim.Walk.name)
            {
                _animation.CrossFade(anim.Walk.name, 0.3f);
                currentAnimation = anim.Walk.name;
            }
        }
        else
        {
            // �ִϸ��̼� ��ȯ
            if (currentAnimation != anim.Idle.name)
            {
                _animation.CrossFade(anim.Idle.name, 0.3f);
                currentAnimation = anim.Idle.name;
            }
        }
    }
    #endregion

    #region Ű����θ� �̵�
    void KeyMove()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        if (0.0f != h || v > 0)
        {
            ClearMsPick();

            m_CacRot = transform.eulerAngles.y;
            m_CacRot += h * m_RotKBSpeed * Time.deltaTime;
            transform.eulerAngles = new Vector3(0, m_CacRot, 0);

            Vector3 a_MoveVec = transform.forward * v;
            a_MoveVec.y = 0;
            transform.position += a_MoveVec.normalized * m_MoveVel * Time.deltaTime;

            // ĳ���Ͱ� �̵� ������ �ٶ󺸵��� ȸ��
            if (a_MoveVec != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(a_MoveVec);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_RotSpeed);
            }
        }

        //�Է°��� ���� �ִϸ��̼� ����
        if (v >= 0.1f || h >= 0.1f)
        {
            if (currentAnimation != anim.Walk.name)
            {
                _animation.CrossFade(anim.Walk.name, 0.3f);
                currentAnimation = anim.Walk.name;
            }
        }
        else
        {
            if (currentAnimation != anim.Idle.name)
            {
                _animation.CrossFade(anim.Idle.name, 0.3f);
                currentAnimation = anim.Idle.name;
            }
        }
    }
    #endregion
}
