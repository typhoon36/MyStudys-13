using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserNodeItem : MonoBehaviour
{
    [HideInInspector] public int m_UniqID = -1;       //������ ������ȣ
    [HideInInspector] public string m_TeamKind = "";  //��
    [HideInInspector] public bool m_IamReady = false; //Ready����

    //Tank �̸� ǥ���� Text UI �׸�
    public Text TankNameText;
    //Ready ���� ǥ�ø� ���� Text UI �׸�
    public Text StateInfoText;

    //// Start is called before the first frame update
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public void DispPlayerData(string a_TankName, bool isMine = false)
    {
        if (isMine == true)
        {
            TankNameText.color = Color.magenta;
            TankNameText.text = a_TankName;
        }
        else
            TankNameText.text = a_TankName;

        if (m_IamReady == true)
            StateInfoText.text = "<color=#ff0000>Ready</color>";
        else
            StateInfoText.text = "";
    }
}
