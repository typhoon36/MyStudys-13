using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalValue
{
    public static string g_Unique_ID = "";  //������ ������ȣ

    public static string g_NickName = "";   //������ ����
    public static int g_BestScore = 0;      //��������
    public static int g_UserGold = 0;       //���ӸӴ�

    public static void ClearGameData()
    {
        g_Unique_ID = "";   //���� ������ȣ
        g_NickName  = "";   //������ ����
        g_BestScore = 0;    //��������
        g_UserGold  = 0;    //���ӸӴ�      
    }

}
