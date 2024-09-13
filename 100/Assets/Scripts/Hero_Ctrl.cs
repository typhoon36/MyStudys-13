using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hero_Ctrl : MonoBehaviour
{
    //# ü��
    float m_MaxHP = 100.0f;
    [HideInInspector]public float m_CurHP = 100.0f;
    public Image m_HPBar;


    //# Ű���� �̵� ���� ���� ����
    float h, v;                 //Ű���� �Է°��� �ޱ� ���� ����
    float m_MoveSpeed = 2.8f;   //�ʴ� 2.8m �̵� �ӵ�

    Vector3 m_DirVec;           //�̵��Ϸ��� ���� ���� ����


    Rigidbody2D Rd;

    Animator m_Anim;            //Animator ������Ʈ�� ������ ����

    //# �Ѿ� �߻�
    public GameObject m_Bullet;
    float m_ShotCool = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_Anim = GetComponentInChildren<Animator>();    //Animator ������Ʈ ã�ƿ���...
        Rd = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_CurHP <= 0.0f) return;

        KeyBDUpdate();
        ChangeAnimation();
        BulletUpdate();
    }

    void KeyBDUpdate()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        if (h != 0 || v != 0) //�̵� Ű���带 �����ϰ� ������...
        {
            //  m_DirVec = (transform.right * h) + (transform.up * v);
            m_DirVec = (Vector3.right* h) + (Vector3.up* v);
            if (1.0f < m_DirVec.magnitude)
                m_DirVec.Normalize();

            //transform.position += (m_DirVec * m_MoveSpeed * Time.deltaTime);
            Rd.velocity = m_DirVec * m_MoveSpeed;
        }
        else  // ���� ���� ��
        {
            m_DirVec = Vector3.zero;
            Rd.velocity = Vector3.zero;
        }
    }//void KeyBDUpdate()

    void ChangeAnimation()
    {
        if (0.01f < m_DirVec.magnitude) //�̵� ���� ��
        {
            if (Mathf.Abs(m_DirVec.x) > Mathf.Abs(m_DirVec.y))  //�¿� �̵�
            {
                if (m_DirVec.x > 0) //���������� �̵� ���� ��
                    m_Anim.Play("Warrior_Right_Walk");
                else  //�������� �̵� ���� ��
                    m_Anim.Play("Warrior_Left_Walk");
            }
            else  //���� �̵�
            {
                if (m_DirVec.y < 0)
                    m_Anim.Play("Warrior_Front_Walk");
                else
                    m_Anim.Play("Warrior_Back_Walk");
            }

            m_Anim.speed = 0.8f;    //�ִϸ��̼� �÷��� �ӵ� ����  
        }
        else  //���� ���� ��
        {
            m_Anim.speed = 0.0f;    //�ִϸ��̼� �ӵ��� 0���� �����Ͽ� ����
        }
    }

    void BulletUpdate()
    {
        if (0.0f < m_ShotCool)
            m_ShotCool -= Time.deltaTime;

        if (Input.GetMouseButton(0))
        {
            if (m_ShotCool <= 0.0f)
            {
                m_ShotCool = 0.2f;

                Vector3 a_TargetPos = Input.mousePosition;
                a_TargetPos = Camera.main.ScreenToWorldPoint(a_TargetPos);

                a_TargetPos.z = 0;
                Vector3 a_DirVec = a_TargetPos - transform.position;
                a_DirVec.Normalize();

                GameObject a_BulletObj = Instantiate(m_Bullet);
                Bullet_Ctrl a_BulletSc = a_BulletObj.GetComponent<Bullet_Ctrl>();
                a_BulletSc.BulletSpawn(transform.position, a_DirVec);

                //�Ѿ�ȸ��
                a_BulletObj.transform.right = new Vector3(a_DirVec.x, a_DirVec.y, 0);
            }
        }
    }

    public void TakeDamage(float a_Val)
    {
        if (m_CurHP <= 0.0f) return;

        m_CurHP -= a_Val;

        if(m_CurHP <= 0.0f)
            m_CurHP = 0.0f;

        if(m_HPBar != null)
            m_HPBar.fillAmount = m_CurHP / m_MaxHP;

        if (m_CurHP <= 0.0f)
        {
            Time.timeScale = 0.0f;
            //����
           
        }
          
        
    }
}
