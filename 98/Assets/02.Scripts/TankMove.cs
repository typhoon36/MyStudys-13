using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMove : MonoBehaviour
{
    public float m_MoveSpeed = 20.0f;
    public float m_RotSpeed = 50.0f;

    //# 참조 컴포넌트
    Rigidbody m_Rd;
    Transform Tr;

    //# 키보드
    float h, v;

    void Start()
    {
        m_Rd = GetComponent<Rigidbody>();
        Tr = GetComponent<Transform>();

        //## rigidbody의 무게중심 낮게 설정
        m_Rd.centerOfMass = new Vector3(0, -2.5f, 0);
        
    }

    // Update is called once per frame
    void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        //## 회전 및 이동
        Tr.Rotate(Vector3.up * m_RotSpeed * h * Time.deltaTime);
        Tr.Translate(Vector3.forward * v * m_MoveSpeed * Time.deltaTime);

    }
}
