using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_Ctrl : MonoBehaviour
{
    Vector3 m_Dir = Vector3.right;

    float m_Speed = 15.0f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 3.0f);
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += m_Dir * m_Speed * Time.deltaTime;
        
        //## 화면밖으로 나가면 삭제
        if (Cam_Resol.m_VPW_Max.x + 1.0f < transform.position.x ||
            Cam_Resol.m_VPW_Min.x - 1.0f > transform.position.x ||
            Cam_Resol.m_VPW_Max.y + 1.0f < transform.position.y ||
            Cam_Resol.m_VPW_Min.y - 1.0f > transform.position.y)
        {
            Destroy(gameObject);
        }
    }

    public void BulletSpawn(Vector3 a_StPos, Vector3 a_DirVec, float a_Speed = 15f)
    {
        transform.position = new Vector3(a_StPos.x, a_StPos.y, 0);
        m_Dir = a_DirVec;
        m_Speed = a_Speed;
    }
}
