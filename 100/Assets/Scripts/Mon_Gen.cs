using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mon_Gen : MonoBehaviour
{
    public GameObject m_Mob1;
    public GameObject m_Mob2;
    public GameObject m_Mob3;
    public GameObject m_Mob4;
    public GameObject m_Mob5;


    float m_SPDelta = 0.0f;
    float m_DiffSpawn = 1.0f;

    // Start is called before the first frame update
    void Start()
    {



    }

    // Update is called once per frame
    void Update()
    {
        //# 스폰주기 설정
        m_SPDelta -= Time.deltaTime;

        if (m_SPDelta < 0.0f)
        {
            m_SPDelta = m_DiffSpawn;

            //## 몬스터 생성
            GameObject a_Mob = Instantiate(m_Mob1);
            Vector3 a_SpPos = Vector3.zero;
            int side = Random.Range(0, 4);

            switch (side)
            {
                case 0: //왼쪽

                    a_SpPos = new Vector3(Random.Range(Cam_Resol.m_VPW_Min.x - 2.0f, Cam_Resol.m_VPW_Max.x - 0.5f),
                        Random.Range(Cam_Resol.m_VPW_Max.y - 2.0f, Cam_Resol.m_VPW_Max.y + 2.0f), 0);


                    break;

                case 1:

                    a_SpPos = new Vector3(Random.Range(Cam_Resol.m_VPW_Max.x + 0.5f, Cam_Resol.m_VPW_Max.x + 2.0f),
                                               Random.Range(Cam_Resol.m_VPW_Min.y - 2.0f, Cam_Resol.m_VPW_Max.y + 2.0f), 0);

                    break;

                case 2:
                    a_SpPos = new Vector3(Random.Range(Cam_Resol.m_VPW_Min.x - 2.0f, Cam_Resol.m_VPW_Max.x + 2.0f),
                                          Random.Range(Cam_Resol.m_VPW_Min.y - 2.0f, Cam_Resol.m_VPW_Min.y - 0.5f), 0);

                    break;

                case 3:

                    a_SpPos = new Vector3(Random.Range(Cam_Resol.m_VPW_Min.x - 2.0f, Cam_Resol.m_VPW_Max.x + 2.0f),
                                                                 Random.Range(Cam_Resol.m_VPW_Max.y + 0.5f, Cam_Resol.m_VPW_Max.y + 2.0f), 0);

                    break;





            }

            a_Mob.transform.position = a_SpPos;

        }
    }
}
