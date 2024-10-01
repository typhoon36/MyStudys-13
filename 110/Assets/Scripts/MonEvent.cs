using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonEvent : MonoBehaviour
{
    Mon_Ctrl m_MonCtrl;

    void Start()
    {
        m_MonCtrl = transform.parent.GetComponent<Mon_Ctrl>();
    }

    void Even_AttHit()
    {
        m_MonCtrl.EventAttDmg();
        
    }
   
}
