using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    Transform m_CamTr;
    
    void Start()
    {
        m_CamTr = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.forward = m_CamTr.forward;
    }
}
