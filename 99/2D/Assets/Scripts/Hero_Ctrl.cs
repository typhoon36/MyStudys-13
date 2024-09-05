using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero_Ctrl : MonoBehaviour
{
    //--- Ű���� �̵� ���� ���� ����
    float h, v;                 //Ű���� �Է°��� �ޱ� ���� ����
    float m_MoveSpeed = 2.8f;   //�ʴ� 2.8m �̵� �ӵ�

    Vector3 m_DirVec;           //�̵��Ϸ��� ���� ���� ����
    //--- Ű���� �̵� ���� ���� ����

    Animator m_Anim;            //Animator ������Ʈ�� ������ ����

    // Start is called before the first frame update
    void Start()
    {
        m_Anim = GetComponentInChildren<Animator>();    //Animator ������Ʈ ã�ƿ���...
    }

    // Update is called once per frame
    void Update()
    {
        KeyBDUpdate();
        ChangeAnimation();
    }

    void KeyBDUpdate()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        if(h != 0 || v != 0) //�̵� Ű���带 �����ϰ� ������...
        {
            m_DirVec = (transform.right * h) + (transform.up * v);
            if (1.0f < m_DirVec.magnitude)
                m_DirVec.Normalize();

            transform.position += (m_DirVec * m_MoveSpeed * Time.deltaTime);
        }
        else  // ���� ���� ��
        {
            m_DirVec = Vector3.zero;
        }
    }//void KeyBDUpdate()

    void ChangeAnimation()
    {
        if(0.01f < m_DirVec.magnitude) //�̵� ���� ��
        {
            if(Mathf.Abs(m_DirVec.x) > Mathf.Abs(m_DirVec.y))  //�¿� �̵�
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
}
