using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPoolUnit : MonoBehaviour
{
    public float m_Delay = 1.0f;    //Ǯ�� ȯ���ǰ� ��� 1�� ���� �͵� ����ؾ� ��
    DateTime m_InactiveTime;
    //Active ���� ���� �ð� : ������ ������ 1�� ���� �� üũ ���� ���
    EffectPool m_ObjectPool;
    string m_EffectName;

    //--- PaticleAutoDestroy�� ���� �ʿ��� Ÿ��
    public enum DESTROY_TYPE
    {
        Destroy,
        Inactive,
    }

    DESTROY_TYPE m_DelType = DESTROY_TYPE.Inactive;
    //Ǯ�� ȯ���ϴ°� ��Ģ�̶� �⺻�� Inactive
    float m_LifeTime = 0.0f;
    //�Ȳ����� Loop ���� ��ƼŬ�� ������ �Ȳ����� ��ƼŬ�� �����ϱ� ����
    //LifeTime ���� ����(���ӽð�) ���
    //LifeTime�� ���� �ִٸ� �̺����� ����... ���ٸ� �׳� isPlaying���� ����
    float m_curLifeTime;
    ParticleSystem[] m_Particles;

    // Start is called before the first frame update
    void Start()
    {
        m_Particles = GetComponentsInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_LifeTime > 0)
        {
            m_curLifeTime += Time.deltaTime;
            if(m_LifeTime <= m_curLifeTime)
            {
                DestroyParticles();
                m_curLifeTime = 0;
            }
        }
        else
        {
            bool isPlay = false;
            for(int i = 0; i < m_Particles.Length; i++)
            {
                if (m_Particles[i].isPlaying == true) //��ƼŬ ��������� üũ����
                {
                    isPlay = true;
                    break;
                }
            }
            if(isPlay == false)
            {
                DestroyParticles();
            }
        }//else
    }//void Update()

    void DestroyParticles()
    {
        switch(m_DelType)
        {
            case DESTROY_TYPE.Destroy:
                Destroy(gameObject);
                break;
            case DESTROY_TYPE.Inactive:
                gameObject.SetActive(false);
                break;
        }
    }//void DestroyParticles()

    public void SetObjectPool(string effectName, EffectPool objectPool)
    {
        m_EffectName = effectName;  //� ����Ʈ����?
        m_ObjectPool = objectPool;  //� Ǯ���� �����ϴ� ����Ʈ����?
        ResetParent();
    }

    public void ResetParent()
    {
        transform.SetParent(m_ObjectPool.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public bool IsReady()
    {
        if (gameObject.activeSelf == false)  //���� ������ Ǯ�� �� ������ �ǹ�
        {
            TimeSpan timeSpan = DateTime.Now - m_InactiveTime;
            //����ð� - ��Ƽ�� ���� �� �ð� //timeSpan���� ���� ����
            if(timeSpan.TotalSeconds > m_Delay)
            {   //�ð��� ��ü �� / �� / �ʷ� ������ ���� �� �ִ�.
                //timeSpan.TotalSeconds �ʷθ� ��ȯ�� ���� �� 1�ʺ��� ũ��

                //��Ƽ�갡 ������ 1���̻� ������ ����Ʈ
                //������ ��Ʈ���� ������ �߻��ȵǼ� 1�� ���� ����
                return true;
            }
        }

        return false;
    }//public bool IsReady()

    private void OnDisable()
    {  // ��Ȱ��ȭ �� ������ ȣ��Ǵ� �Լ��� (��ũ��Ʈ��, ������Ʈ��)
        m_InactiveTime = DateTime.Now;
        m_ObjectPool.AddPoolUnit(m_EffectName, this);
        //��Ƽ�갡 ���� �� �޸�Ǯ�� �ٽ� �־���
    }
}
