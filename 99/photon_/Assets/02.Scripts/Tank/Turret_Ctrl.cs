using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret_Ctrl : MonoBehaviour
{

    //## Transform ������Ʈ�� ������ ����
    Transform tr;

    //## ����ĳ��Ʈ
    RaycastHit hit;

    //## ȸ���ӵ�
    public float m_RotSpeed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        tr = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        //## ���콺 Ŀ���� ��ġ�� �������� ����ĳ��Ʈ ��� �ٶ󺸰�.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.red);

        //## ����ĳ��Ʈ�� �ε����� hit�� ����
        if (Physics.Raycast(ray, out hit, 100.0f, 1<< 6))
        {
            //### �ε��� ������ �������� ȸ��
            Vector3 relative = tr.InverseTransformPoint(hit.point);
            //### Atan2 �Լ��� �̿��� ���� ���
            float angle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
            //### ȸ��
            tr.Rotate(0, angle * Time.deltaTime* m_RotSpeed, 0);
        }

    }
}