using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffUnit_Pool : MonoBehaviour
{
    [HideInInspector] public float m_EffDelay = 1.0f;
    DateTime m_Inactive_Time; //비활성화시의 시간

    EffPool m_Obj;

    string m_EffName;

    public enum Destroy_Type { Destroy, Interactive }

    Destroy_Type m_DelType = Destroy_Type.Interactive;

    float m_LifeTime = 0.0f;//안꺼지고 돌고있을때 제어    

    float m_CurTime;

    ParticleSystem[] m_Particles;

    void Start()
    {
        m_Particles = GetComponentsInChildren<ParticleSystem>();

    }

    void Update()
    {
        if (m_LifeTime > 0)
        {
            m_CurTime += Time.deltaTime;
            if (m_CurTime <= m_LifeTime)
            {
                DestroyParticles();
                m_CurTime = 0;
            }
        }
        else
        {
            bool IsAlive = false;
            for (int i = 0; i< m_Particles.Length; i++)
            {
                //재생중인지 체크(하나라도 재생중이면 IsAlive = true)
                if (m_Particles[i].isPlaying == true)
                {
                    IsAlive = true;
                    break;
                }
            }

            //재생중이 아니면 파티클 제거
            if (IsAlive == false)
            {
                DestroyParticles();
            }
        }

    }

    void DestroyParticles()
    {
        switch (m_DelType)
        {
            case Destroy_Type.Destroy:
                Destroy(gameObject);
                break;
            case Destroy_Type.Interactive:
                gameObject.SetActive(false);
                break;
        }

    }

    public void SetObjPool(string a_EffName, EffPool a_ObjPool)
    {
        m_Obj = a_ObjPool;
        m_EffName = a_EffName;
        ResetParent();
    }

    public void ResetParent()
    {
        transform.SetParent(m_Obj.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

    }

    public bool IsReady()
    {
        if (gameObject.activeSelf == false)
        {
            TimeSpan a_TimeS = DateTime.Now - m_Inactive_Time;

            if (a_TimeS.TotalSeconds >= m_EffDelay)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDisable()
    {
        m_Inactive_Time = DateTime.Now;

        m_Obj.AddPool(m_EffName, this);

    }

}
