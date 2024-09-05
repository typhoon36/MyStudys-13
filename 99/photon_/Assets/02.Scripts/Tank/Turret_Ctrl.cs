using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret_Ctrl : MonoBehaviour
{

    //## Transform 컴포넌트를 저장할 변수
    Transform tr;

    //## 레이캐스트
    RaycastHit hit;

    //## 회전속도
    public float m_RotSpeed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        tr = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        //## 마우스 커서의 위치를 기준으로 레이캐스트 쏘고 바라보게.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.red);

        //## 레이캐스트가 부딪히면 hit에 저장
        if (Physics.Raycast(ray, out hit, 100.0f, 1<< 6))
        {
            //### 부딪힌 지점을 기준으로 회전
            Vector3 relative = tr.InverseTransformPoint(hit.point);
            //### Atan2 함수를 이용해 각도 계산
            float angle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
            //### 회전
            tr.Rotate(0, angle * Time.deltaTime* m_RotSpeed, 0);
        }

    }
}