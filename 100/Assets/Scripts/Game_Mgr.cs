using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_Mgr : MonoBehaviour
{
    int m_CurPoint = 0;
    public Text m_PointTxt;

    //# Singleton pattern
    public static Game_Mgr Inst = null;

    void Awake()
    {

        Inst = this;


    }



    void Start()
    {
        

    }

    void Update()
    {

    }



    public void AddPoint(int a_Val = 5)
    {
        m_CurPoint += a_Val;

        m_PointTxt.text = "Point : " + m_CurPoint;
    }

}
