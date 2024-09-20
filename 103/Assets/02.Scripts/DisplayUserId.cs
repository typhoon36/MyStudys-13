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

    public void ChangeTN_Color(GameMgr a_GMr)
    {
        if(pv == null) return;

        if (a_GMr == null) return;

        string a_TeamKnd = a_GMr.ReciveSelTm(pv.Owner);

        if(a_TeamKnd == "Blue")
            userId.color = Color.blue;
        else if(a_TeamKnd == "Red")
            userId.color = Color.red;
        

    }
}
