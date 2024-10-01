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


    #region 키보드 이동
    float h = 0, v = 0;

    float m_CacRotY = 0.0f;
    float m_RotKbSpeed = 150.0f;    //초당 150도 회전하려는 속도
    #endregion

    float m_MoveVelocity = 5.0f;  //평면 초당 이동 속도...

    //--- JoyStick 이동 처리 변수
    float m_JoyMvLen = 0.0f;
    Vector3 m_JoyMvDir = Vector3.zero;
    //--- JoyStick 이동 처리 변수

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

    //--- Animator 관련 변수
    Animator m_Animator = null;
    AnimState m_PreState = AnimState.idle;
    AnimState m_CurState = AnimState.idle;
    //--- Animator 관련 변수


    #region 공격
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
        h = Input.GetAxisRaw("Horizontal"); //화살표키 좌우키를 누르면 -1.0f ~ 1.0f
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
            // 마우스 픽 이동 클리어
            ClearMsPickMove();

            // 카메라 방향에 따른 이동 방향 계산
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

        // 조이스틱 이동 처리
        if (0.0f < m_JoyMvLen)
        {
            m_TargetE = null;

            if (IsSkill() == true) return;

            m_MoveDir = m_JoyMvDir;

            // 캐릭터 회전
            if (0.0001f < m_JoyMvDir.magnitude)
            {
                m_TargetRot = Quaternion.LookRotation(m_JoyMvDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                        m_TargetRot, Time.deltaTime * m_RotSpeed);
            }

            transform.position += m_JoyMvDir * (m_MoveVelocity * Time.deltaTime);
        }
    }

    void MousePickCheck()   //마우스 클릭 감지를 위한 함수
    {
        if (Input.GetMouseButtonDown(0) == true) //왼쪽 마우스 버튼 클릭시
            if (GameMgr.IsPointerOverUIObject() == false)    //UI가 아닌 곳을 클릭했을 때만 피킹 이동 허용
            {
                if (IsSkill() == true) return;

                m_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(m_MousePos, out hitInfo, Mathf.Infinity, LayerMask.value))
                {
                    MousePicking(hitInfo.point);
                    GameMgr.Inst.CurMarkOn(hitInfo.point);
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


        m_TargetE = a_PickMon;
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

        if (GameMgr.Inst.m_CursorMark != null)
            GameMgr.Inst.m_CursorMark.SetActive(false);

        //마우스 클릭 취소
    }//void ClearMsPickMove()

    //애니메이션 상태 변경 메서드
    public void ChangeAnimState(AnimState newState, float crossTime = 0.1f, string animName = "")
    {
        if (m_Animator == null)
            return;

        if (m_PreState == newState)
            return;

        m_Animator.ResetTrigger(m_PreState.ToString()); //기존에 적용되어 있던 Trigger 변수를 제거

        if (0.0f < crossTime)
        {
            m_Animator.SetTrigger(newState.ToString());
        }
        else
        {
            m_Animator.Play(animName, -1, 0);
            //가운데 -1은 Layer Index, 뒤에 0는 처음부터 다시 시작 플레이 시키겠다는 의미
        }

        m_PreState = newState;
        m_CurState = newState;
    }

    //애니메이션 상태 업데이트 메서드
    void UpdateAnimState()
    {
        if (IsAttack() == true) return;



        //모든 조작 이동이 아니면 idle로 변경
        if ((0 == h&& 0 == v) && 0 == m_JoyMvLen && m_IsPickMoveOnOff == false && !IsAttack())
        {
            ChangeAnimState(AnimState.idle);
        }
        //맞으면 달리기로
        else
        {
            ChangeAnimState(AnimState.move);
        }
    }

    public void Attack()
    {
        if (!IsAttack())
        {
            //이동 중이면서 공격키를 연타해서 누르면 공격 애니가 끼어듬.
            //방지차 코드
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
        //if(m_CurState == AnimState.attack) return;//attack 상태에서만 스킬 사용 불가능

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
        //타겟 비활성화 시 false
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

        //정밀한 공격을 위한 예외 처리
        if (IsAttack() == false) return;

        // 공격 애니메이션 중 타겟 무효화시
        if (IsTargetE_Active(0.5f) == false)
            FindE_Target();


    }
    void FindE_Target()
    {
        m_EList = GameObject.FindGameObjectsWithTag("Enermy");

        float a_MinLen = float.MaxValue;
        int m_ICnt = m_EList.Length;

        m_TargetE = null;
        for (int i = 0; i < m_ICnt; i++) // 여기서 a_MinLen 대신 m_ICnt를 사용합니다.
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

    #region 데미지 처리
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
