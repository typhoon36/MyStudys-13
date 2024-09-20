using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserNdItem : MonoBehaviour
{
    //# 이름 표시
    public Text m_TxtTankName;

    //#Ready Text  
    public Text m_StateTxt;

    [HideInInspector] public int m_Uniq_ID = -1;
    [HideInInspector] public string m_TeamKnd = "";
    [HideInInspector] public bool IsReady = false;

    public void DispPlayerData(string a_TkName, bool IsMine)
    {
        if (IsMine)
        {
            m_TxtTankName.color = Color.magenta;
            m_TxtTankName.text = a_TkName;
        }
        else
        {
            m_TxtTankName.text = a_TkName;
        }

        if (IsReady)
        {
            m_StateTxt.text = "<color=#ff0000>Ready</color>";
        }
        else
        {
            m_StateTxt.text = "";
        }
    }
}
