using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Ctrl : MonoBehaviour
{
    GameObject m_Player = null;
    Vector3 m_TargetPos = Vector3.zero;

    //--- ī�޶� ��ġ ���� ����
    float m_RotH = 0.0f;        //���콺 �¿� ���۰� ���� ����
    float m_RotV = 0.0f;        //���콺 ���� ���۰� ���� ����
    float hSpeed = 5.0f;        //���콺 �¿� ȸ���� ���� ī�޶� ȸ�� ���ǵ� ������
    float vSpeed = 2.4f;        //���콺 ���� ȸ���� ���� ī�޶� ȸ�� ���ǵ� ������
    float vMinLimit = -7.0f;    //�� �Ʒ� ���� ����
    float vMaxLimit = 80.0f;    //�� �Ʒ� ���� ����
    float zoomSpeed = 1.0f;     //���콺 �� ���ۿ� ���� ���ξƿ� ���ǵ� ������
    float minDist = 3.0f;       //���콺 �� �� �ּ� �Ÿ� ���Ѱ�
    float maxDist = 50.0f;      //���콺 �� �ƿ� �ִ� �Ÿ� ���Ѱ�
    //--- ī�޶� ��ġ ���� ����

    //--- ���ΰ��� �������� �� ������� ����ǥ�� ������ �ʱⰪ
    float m_DefaultRotH = 0.0f;    //���� ������ ȸ�� ����
    float m_DefaultRotV = 25.0f;   //���� ������ ȸ�� ����
    float m_DefaltDist = 5.2f;     //Ÿ�ٿ��� ī�޶������ �Ÿ�
    //--- ���ΰ��� �������� �� ������� ����ǥ�� ������ �ʱⰪ

    //--- ��꿡 �ʿ��� ������...
    float m_CurDistance = 17.0f;            // ���� ���ΰ����� ī�޶������ �Ÿ�
    //Quaternion m_BuffRot;
    private float m_TargetDistance;         // ��ǥ ���ΰ����� ī�޶������ �Ÿ�
    private Quaternion m_CurrentRotation;   // ���� ȸ����
    private Quaternion m_TargetRotation;    // ��ǥ ȸ���� ����
    Vector3 m_BasicPos = Vector3.zero;
    Vector3 m_BuffPos = Vector3.zero;
    private float rotationSmoothTime = 0.08f; //0.1f;   // ȸ�� �ε巴�� ��ȭ��Ű�� �ð�
    private float zoomSmoothTime = 0.1f;       // �� �ε巴�� ��ȭ��Ű�� �ð�
    private Vector3 rotationVelocity = Vector3.zero; //ȸ�� �ӵ�
    private float zoomVelocity = 0.0f;               //�� �ӵ�
    //--- ��꿡 �ʿ��� ������...

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

        //--- ī�޶� ��ġ ��� ���� (����ǥ�踦 ������ǥ��� ȯ���ϴ� �κ�)
        m_RotH = m_DefaultRotH; //���� ������ ȸ�� ����
        m_RotV = m_DefaultRotV; //���� ������ ȸ�� ����
        m_CurDistance = m_DefaltDist; //�ʱ� ī�޶� �Ÿ� ����
        m_TargetDistance = m_CurDistance; //������ ��ǥ �Ÿ��� ���� �����Ѵ�.

        m_CurrentRotation = Quaternion.Euler(m_RotV, m_RotH, 0.0f); //���� ȸ����
        m_BasicPos.x = 0.0f;
        m_BasicPos.y = 0.0f;
        m_BasicPos.z = -m_CurDistance;

        m_BuffPos = m_TargetPos + (m_CurrentRotation * m_BasicPos);

        transform.position = m_BuffPos;  //<-- ī�޶��� ������ǥ�� ������ ��ġ
        transform.LookAt(m_TargetPos);
        //--- ī�޶� ��ġ ��� ���� (����ǥ�踦 ������ǥ��� ȯ���ϴ� �κ�)
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (m_Player == null)
            return;

        m_TargetPos = m_Player.transform.position;
        m_TargetPos.y += 1.4f;

        if (Input.GetMouseButton(1) == true) //���콺 ���� ��ư�� ������ �ִ� ����
        {
            //���콺�� �¿�� �������� �� ��
            m_RotH += Input.GetAxis("Mouse X") * hSpeed;
            //���콺�� ���Ʒ��� �������� �� ��
            m_RotV -= Input.GetAxis("Mouse Y") * vSpeed;

            m_RotV = ClampAngle(m_RotV, vMinLimit, vMaxLimit);
        }//if(Input.GetMouseButton(1) == true) //���콺 ���� ��ư�� ������ �ִ� ����

        //ȸ�� �ε巴�� ���� 
        m_TargetRotation = Quaternion.Euler(m_RotV, m_RotH, 0.0f);
        m_CurrentRotation = Quaternion.Slerp(m_CurrentRotation, m_TargetRotation,
                                                Time.deltaTime / rotationSmoothTime);
        //Time.deltaTime / rotationSmoothTime�� ��ü ȸ�� �ð� ���
        //���� �����ӿ��� �����ؾ� �� ������ ����մϴ�.
        //���� ����
        //ȸ���� �ε巴�� �ϱ� ���ؼ��� ��ü ȸ�� �ð��� �������� �� �����Ӹ���
        //������ ������ ȸ���ؾ� �մϴ�.
        //���� ���, rotationSmoothTime�� 0.5�ʶ��, ȸ���� 0.5�ʿ� ���� �Ϸ�Ǿ�� �մϴ�.
        //�� �����Ӹ��� �����ؾ� �� ȸ�� ���� t�� Time.deltaTime�� ��ü ȸ�� �ð����� ���� ���Դϴ�.

        // ���콺 �� �Է� ó��
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0.0f)
        {
            // * 20f ���� : ���콺 �� �Է� ���� �������� �����Ͽ� �� ��/ �ƿ��� ������ �������ϴ�.
            m_TargetDistance -= scrollInput * zoomSpeed * 20f;
            m_TargetDistance = Mathf.Clamp(m_TargetDistance, minDist, maxDist);
        }

        // �� �ε巴�� ����
        m_CurDistance = Mathf.SmoothDamp(m_CurDistance, m_TargetDistance, ref zoomVelocity, zoomSmoothTime);
        //Mathf.SmoothDamp �Լ��� ����Ͽ� ���� �Ÿ����� ��ǥ �Ÿ��� �ε巴�� ��ȭ��ų �� �ֽ��ϴ�.

        m_BasicPos.x = 0.0f;
        m_BasicPos.y = 0.0f;
        m_BasicPos.z = -m_CurDistance;

        m_BuffPos = m_TargetPos + (m_CurrentRotation * m_BasicPos);

        transform.position = m_BuffPos; //<--- ī�޶��� ���� ��ǥ�� ������ ��ġ
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
