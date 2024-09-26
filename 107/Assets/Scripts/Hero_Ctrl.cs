using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero_Ctrl : MonoBehaviour
{
    //--- 키보드 이동 관련 변수 선언
    float h = 0, v = 0;

    float m_CacRotY = 0.0f;
    float m_RotKbSpeed = 150.0f;    //초당 150도 회전하려는 속도
                                    //--- 키보드 이동 관련 변수 선언

    float m_MoveVelocity = 5.0f;  //평면 초당 이동 속도...

    #region JoyStick 
    float m_JoyMvLen = 0.0f;
    Vector3 m_JoyMvDir = Vector3.zero;
    #endregion

    //--- Picking 관련 변수
    Ray m_MousePos;
    RaycastHit hitInfo;
    LayerMask LayerMask = -1;

    bool m_IsPickMoveOnOff = false;     //피킹 이동 OnOff
    Vector3 m_TargetPos = Vector3.zero; //최종 목표 위치
    Vector3 m_MoveDir = Vector3.zero;   //x, z평면 진행 방향
    double m_MoveDurTime = 0;           //목표점가지 도착하는데 걸리는 시간
    double m_AddTimeCount = 0;          //누적 시간 카운트
    Vector3 m_CacLenVec = Vector3.zero; //이동 계산용 변수
    Quaternion m_TargetRot = Quaternion.identity; //회전 계산용 변수
    float m_RotSpeed = 7.0f;            //초당 7도 회전하려는 속도
                                        //--- Picking 관련 변수

    //# Animator 
    Animator m_Anim = null;
    AnimState m_PreState = AnimState.idle;
    AnimState m_CurState = AnimState.idle;

    // 공격 관련 변수
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
        h = Input.GetAxisRaw("Horizontal"); //화살표키 좌우키를 누르면 -1.0f ~ 1.0f
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

            //카메라가 보는 방향으로회전
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

            //캐릭터를 이동방향으로 회전
            if (0.0001f < m_JoyMvDir.magnitude)
            {
                m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                       m_TargetRot, Time.deltaTime * m_RotSpeed);
            }

            transform.position += m_JoyMvDir * (m_MoveVelocity * Time.deltaTime);
        }
    }

    void MousePickCheck()   //마우스 클릭 감지를 위한 함수
    {
        if (Input.GetMouseButtonDown(0) == true) //왼쪽 마우스 버튼 클릭시
            if (Game_Mgr.IsPointerOverUIObject() == false)
            {
                m_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(m_MousePos, out hitInfo, Mathf.Infinity, LayerMask.value))
                {
                    MousePicking(hitInfo.point);
                }
            }//if(Input.GetMouseButtonDown(0) == true) //왼쪽 마우스 버튼 클릭시
    }//void MousePickCheck()   //마우스 클릭 감지를 위한 함수

    void MousePicking(Vector3 a_PickVec, GameObject a_PickMon = null) //마우스 클릭 처리 함수
    {
        a_PickVec.y = transform.position.y;     //목표 위치
        Vector3 a_StartPos = transform.position;    //출발 위치

        m_CacLenVec = a_PickVec - a_StartPos;
        m_CacLenVec.y = 0.0f;

        if (m_CacLenVec.magnitude < 0.5f)   //너무 근거리 피킹은 스킵해 준다.
            return;

        m_TargetPos = a_PickVec;    //최종 목표 위치
        m_IsPickMoveOnOff = true;   //피킹 이동 OnOff

        m_MoveDir = m_CacLenVec.normalized;
        m_MoveDurTime = m_CacLenVec.magnitude / m_MoveVelocity; //도착하는데까지 걸리는 시간
        m_AddTimeCount = 0.0;
    }

    void MousePickUpdate()  //마우스 클릭으로 캐릭터 이동을 계산하는 함수
    {
        if (m_IsPickMoveOnOff == true)
        {
            m_CacLenVec = m_TargetPos - transform.position;
            m_CacLenVec.y = 0.0f;

            m_MoveDir = m_CacLenVec.normalized;

            //캐릭터를 이동방향으로 회전시키는 코드
            if (0.0001f < m_CacLenVec.magnitude)
            {
                m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                    m_TargetRot, Time.deltaTime * m_RotSpeed);
            }
            //캐릭터를 이동방향으로 회전시키는 코드

            m_AddTimeCount += Time.deltaTime;
            if (m_MoveDurTime <= m_AddTimeCount) //목표점에 도착한 것으로 판정한다.
            {
                ClearMsPickMove();
            }
            else
            {
                transform.position += m_MoveDir * Time.deltaTime * m_MoveVelocity;
            }
        }//if (m_IsPickMoveOnOff == true)

    }//void MousePickUpdate()  //마우스 클릭으로 캐릭터 이동을 계산하는 함수

    void ClearMsPickMove()
    {
        m_IsPickMoveOnOff = false; //마우스 클릭 이동 취소

        //마우스 클릭 취소
    }//void ClearMsPickMove()

    #region 애니메이션
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
