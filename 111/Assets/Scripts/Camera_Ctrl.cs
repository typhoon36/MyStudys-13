using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Ctrl : MonoBehaviour
{
    GameObject m_Player = null;
    Vector3 m_TargetPos = Vector3.zero;

    //--- 카메라 위치 계산용 변수
    float m_RotH = 0.0f;        //마우스 좌우 조작값 계산용 변수
    float m_RotV = 0.0f;        //마우스 상하 조작값 계산용 변수
    float hSpeed = 5.0f;        //마우스 좌우 회전에 대한 카메라 회전 스피드 설정값
    float vSpeed = 2.4f;        //마우스 상하 회전에 대한 카메라 회전 스피드 설정값
    float vMinLimit = -7.0f;    //위 아래 각도 제한
    float vMaxLimit = 80.0f;    //위 아래 각도 제한
    float zoomSpeed = 1.0f;     //마우스 휠 조작에 대한 줌인아웃 스피드 설정값
    float minDist = 3.0f;       //마우스 줌 인 최소 거리 제한값
    float maxDist = 50.0f;      //마우스 줌 아웃 최대 거리 제한값
    //--- 카메라 위치 계산용 변수

    //--- 주인공을 기준으로 한 상대적인 구좌표계 기준의 초기값
    float m_DefaultRotH = 0.0f;    //수평 기준의 회전 각도
    float m_DefaultRotV = 25.0f;   //수직 기준의 회전 각도
    float m_DefaltDist = 5.2f;     //타겟에서 카메라까지의 거리
    //--- 주인공을 기준으로 한 상대적인 구좌표계 기준의 초기값

    //--- 계산에 필요한 변수들...
    float m_CurDistance = 17.0f;            // 현재 주인공에서 카메라까지의 거리
    //Quaternion m_BuffRot;
    private float m_TargetDistance;         // 목표 주인공에서 카메라까지의 거리
    private Quaternion m_CurrentRotation;   // 현재 회전값
    private Quaternion m_TargetRotation;    // 목표 회전값 관리
    Vector3 m_BasicPos = Vector3.zero;
    Vector3 m_BuffPos = Vector3.zero;
    private float rotationSmoothTime = 0.08f; //0.1f;   // 회전 부드럽게 변화시키는 시간
    private float zoomSmoothTime = 0.1f;       // 줌 부드럽게 변화시키는 시간
    private Vector3 rotationVelocity = Vector3.zero; //회전 속도
    private float zoomVelocity = 0.0f;               //줌 속도
    //--- 계산에 필요한 변수들...

    public void InitCamera(GameObject a_Player)
    {
        m_Player = a_Player;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_Player == null)
            return;

        m_TargetPos = m_Player.transform.position;
        m_TargetPos.y += 1.4f;

        //--- 카메라 위치 계산 공식 (구좌표계를 직각좌표계로 환산하는 부분)
        m_RotH = m_DefaultRotH; //수평 기준의 회전 각도
        m_RotV = m_DefaultRotV; //수직 기준의 회전 각도
        m_CurDistance = m_DefaltDist; //초기 카메라 거리 적용
        m_TargetDistance = m_CurDistance; //시작은 목표 거리도 같게 시작한다.

        m_CurrentRotation = Quaternion.Euler(m_RotV, m_RotH, 0.0f); //현재 회전값
        m_BasicPos.x = 0.0f;
        m_BasicPos.y = 0.0f;
        m_BasicPos.z = -m_CurDistance;

        m_BuffPos = m_TargetPos + (m_CurrentRotation * m_BasicPos);

        transform.position = m_BuffPos;  //<-- 카메라의 직각좌표계 기준의 위치
        transform.LookAt(m_TargetPos);
        //--- 카메라 위치 계산 공식 (구좌표계를 직각좌표계로 환산하는 부분)
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (m_Player == null)
            return;

        m_TargetPos = m_Player.transform.position;
        m_TargetPos.y += 1.4f;

        if (Input.GetMouseButton(1) == true) //마우스 우측 버튼을 누르고 있는 동안
        {
            //마우스를 좌우로 움직였을 때 값
            m_RotH += Input.GetAxis("Mouse X") * hSpeed;
            //마우스를 위아래로 움직였을 때 값
            m_RotV -= Input.GetAxis("Mouse Y") * vSpeed;

            m_RotV = ClampAngle(m_RotV, vMinLimit, vMaxLimit);
        }//if(Input.GetMouseButton(1) == true) //마우스 우측 버튼을 누르고 있는 동안

        //회전 부드럽게 보간 
        m_TargetRotation = Quaternion.Euler(m_RotV, m_RotH, 0.0f);
        m_CurrentRotation = Quaternion.Slerp(m_CurrentRotation, m_TargetRotation,
                                                Time.deltaTime / rotationSmoothTime);
        //Time.deltaTime / rotationSmoothTime은 전체 회전 시간 대비
        //현재 프레임에서 진행해야 할 비율을 계산합니다.
        //나눈 이유
        //회전을 부드럽게 하기 위해서는 전체 회전 시간을 기준으로 매 프레임마다
        //일정한 비율로 회전해야 합니다.
        //예를 들어, rotationSmoothTime이 0.5초라면, 회전은 0.5초에 걸쳐 완료되어야 합니다.
        //매 프레임마다 진행해야 할 회전 비율 t는 Time.deltaTime을 전체 회전 시간으로 나눈 값입니다.

        // 마우스 휠 입력 처리
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0.0f)
        {
            // * 20f 이유 : 마우스 휠 입력 값에 스케일을 조정하여 줌 인/ 아웃의 감도를 높였습니다.
            m_TargetDistance -= scrollInput * zoomSpeed * 20f;
            m_TargetDistance = Mathf.Clamp(m_TargetDistance, minDist, maxDist);
        }

        // 줌 부드럽게 보간
        m_CurDistance = Mathf.SmoothDamp(m_CurDistance, m_TargetDistance, ref zoomVelocity, zoomSmoothTime);
        //Mathf.SmoothDamp 함수를 사용하여 현재 거리에서 목표 거리로 부드럽게 변화시킬 수 있습니다.

        m_BasicPos.x = 0.0f;
        m_BasicPos.y = 0.0f;
        m_BasicPos.z = -m_CurDistance;

        m_BuffPos = m_TargetPos + (m_CurrentRotation * m_BasicPos);

        transform.position = m_BuffPos; //<--- 카메라의 직각 좌표계 기준의 위치
        transform.LookAt(m_TargetPos);

    }//void LateUpdate()

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360.0f)
            angle += 360.0f;
        if (angle > 360.0f)
            angle -= 360.0f;

        return Mathf.Clamp(angle, min, max);
    }
}
