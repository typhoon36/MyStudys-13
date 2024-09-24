using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayUserId : MonoBehaviour
{
    public Text userId;
    PhotonView pv = null;

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
        userId.text = pv.Owner.NickName;
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public void ChangeTankNameColor(GameMgr a_GameMgr)
    {
        if (pv == null)
            return;

        if (a_GameMgr == null)
            return;

        string a_TeamKind = a_GameMgr.ReceiveSelTeam(pv.Owner);

        //팀이 뭐냐?에 따라서 스크롤 뷰를 분기 해 준다.
        if (a_TeamKind == "blue")
            userId.color = new Color32(60, 60, 255, 255);
        else if (a_TeamKind == "black")
            userId.color = Color.black;

    }//public void ChangeTankNameColor(GameMgr a_GameMgr)
}
