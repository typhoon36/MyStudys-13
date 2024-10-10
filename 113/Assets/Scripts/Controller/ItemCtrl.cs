using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



enum ItemType
{
    HPPotion,
    Diamond
}

public class ItemCtrl : MonoBehaviour
{
    [HideInInspector] public PhotonView pv;
    int m_TakeHId = -1;
    float m_ShowOffTime = 0.0f;
    ItemType m_ItemType = ItemType.HPPotion;

    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (pv != null && pv.IsMine == true && 0 < m_TakeHId)
            PhotonNetwork.Destroy(this.gameObject);

        if (0 < m_ShowOffTime)
        {
            m_ShowOffTime -= Time.deltaTime;
            if (m_ShowOffTime <= 0.0f)
            {
                EnableSwitch(true);
            }
        }

        transform.Rotate(0.0f, Time.deltaTime * 200.0f, 0.0f);
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        if (pv == null) return;

        if (coll.gameObject.tag == "Player")
        {
            Hero_Ctrl a_Hero = coll.gameObject.GetComponent<Hero_Ctrl>();
            if (a_Hero != null)
            {
                pv.RPC("OnTrgItemRPC", RpcTarget.AllViaServer, (int)a_Hero.pv.Owner.ActorNumber,
                    (int)m_ItemType);
            }

            EnableSwitch(false);
            m_ShowOffTime = 5.0f;
        }
    }

    [PunRPC]
    public void OnTrgItemRPC(int a_HId, int a_ItemType)
    {
        if (pv == null) return;

        if (pv.IsMine == false) return;

        if (a_HId < 0) return;

        if (0 <= m_TakeHId) return; // 이미 누군가가 획득한 아이템이다.

        m_TakeHId = a_HId;

        Hero_Ctrl m_RefHero = null;
        GameObject[] a_Heros = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject a_Hero in a_Heros)
        {
            m_RefHero = a_Hero.GetComponent<Hero_Ctrl>();
            if (m_RefHero.pv.Owner.ActorNumber == a_HId)
            {
                m_RefHero.pv.RPC("TakeItemRPC", RpcTarget.AllViaServer, (int)m_ItemType);
                break;
            }
        }

        PhotonNetwork.Destroy(this.gameObject);
    }

    void EnableSwitch(bool IsON)
    {
        MeshRenderer[] a_MeshList = gameObject.GetComponentsInChildren<MeshRenderer>();
        for (int m = 0; m < a_MeshList.Length; m++)
        {
            if (a_MeshList[m] == null) continue;

            a_MeshList[m].enabled = IsON;
        }

        BoxCollider[] a_Coll = gameObject.GetComponentsInChildren<BoxCollider>();

        for (int B = 0; B < a_Coll.Length; B++)
        {
            if (a_Coll[B] == null) continue;

            a_Coll[B].enabled = IsON;
        }
    }
}
