using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Mon_Type
{
    Mon1,
    Mon2,
    Mon3,
    Mon4,
    Mon5,
}


public class Mon_Ctrl : MonoBehaviour
{
    Hero_Ctrl m_RefHero;
    float m_Speed = 1.0f;
    Vector3 m_DirVec;
    Vector3 m_CurPos;

    Animator m_Anim;

    Rigidbody2D Rd;

    // Start is called before the first frame update
    void Start()
    {
        m_RefHero = GameObject.FindObjectOfType<Hero_Ctrl>();
        Rd = GetComponent<Rigidbody2D>();
        m_Anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_RefHero != null)
        {
            m_DirVec = m_RefHero.transform.position - transform.position;
            m_DirVec.Normalize();
            Rd.velocity = m_DirVec * m_Speed;
        }
        else
        {
            m_DirVec = Vector3.zero;
        }

        AnimationCH();
    }

    void AnimationCH()
    {
        if (0.01f < m_DirVec.magnitude) //이동 중일 때
        {
            if (Mathf.Abs(m_DirVec.x) > Mathf.Abs(m_DirVec.y))  //좌우 이동
            {
                if (m_DirVec.x > 0) //오른쪽으로 이동 중일 때
                    m_Anim.Play("Mon1_Right_Walk");
                else  //왼쪽으로 이동 중일 때
                    m_Anim.Play("Mon1_Left_Walk");
            }
            else  //상하 이동
            {
                if (m_DirVec.y < 0)
                    m_Anim.Play("Mon1_Front_Walk");
                else
                    m_Anim.Play("Mon1_Back_Walk");
            }

            m_Anim.speed = 0.8f;    //애니메이션 플레이 속도 조절  
        }
        else  //멈춰 있을 때
        {
            m_Anim.speed = 0.0f;    //애니메이션 속도를 0으로 설정하여 멈춤
        }
    }


    private void OnCollisionEnter2D(Collision2D coll)
    {
        if(coll.gameObject.name.Contains("HeroRoot"))
        {
            Hero_Ctrl a_Hero = coll.gameObject.GetComponent<Hero_Ctrl>();
            if(a_Hero != null)
            {
                a_Hero.TakeDamage(20.0f);
            }
            Destroy(gameObject);

        }
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.tag == "Ally_Bullet")
        {
            Game_Mgr.Inst.AddPoint();

            Destroy(coll.gameObject);
            Destroy(gameObject);
        }
    }
    

}
