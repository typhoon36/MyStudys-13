using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

enum ItemType
{
    HealPotion,
    Diamond
}

public class ItemCtrl : MonoBehaviour
{
    [HideInInspector] public PhotonView pv = null;
    int m_TakeHeroId = -1;  //이 아이템을 획득한 유저의 고유번호
    ItemType m_ItemType = ItemType.Diamond;
    float ShowOffTimer = 0.0f;  //아이템 먹기 중계 실패시 다시 획득할 수 있게 하기 위하여...
    float LifeTime = 10.0f;  //생존 시간

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        //누군가 이미 획득했는데 아직도 이 아이템이 삭제되지 않았다면...
        if (pv != null && pv.IsMine == true && 0 <= m_TakeHeroId)
            PhotonNetwork.Destroy(this.gameObject); //즉시 삭제 중계

        //타임 오버
        if (pv != null && pv.IsMine == true && m_TakeHeroId < 0)
        {
            LifeTime -= Time.deltaTime;
            if (LifeTime <= 0.0f)
            {
                PhotonNetwork.Destroy(this.gameObject);
            }
        }

        if (0.0f < ShowOffTimer)
        {
            ShowOffTimer -= Time.deltaTime;
            if(ShowOffTimer <= 0.0f)
            {
                EnableOnOff(true);  //아이템 먹기 중계 실패시 다시 획득할 수 있게 하기 위하여...
            }
        }//if(0.0f < ShowOffTimer)

        transform.Rotate(0.0f, Time.deltaTime * 200.0f, 0.0f);
    }

    void OnTriggerEnter(Collider coll)
    {
        if(PhotonNetwork.CurrentRoom == null)
            return;

        if (pv == null)
            return;

        if(coll.gameObject.tag == "Player")
        {
            Hero_Ctrl a_RefHero = coll.gameObject.GetComponent<Hero_Ctrl>();
            if(a_RefHero != null)
            {
                pv.RPC("OnTrigItemRPC", RpcTarget.AllViaServer,
                    (int)a_RefHero.pv.Owner.ActorNumber,
                    (int)m_ItemType);
            }

            EnableOnOff(false);
            ShowOffTimer = 5.0f;
        }//if(coll.gameObject.tag == "Player")
    }//void OnTriggerEnter(Collider coll)

    [PunRPC]
    public void OnTrigItemRPC(int a_TakeHeroId, int ItemType)
    { //지금은 모든 Pc에서 충돌이 되고 우선 모두 중계를 받게 될 것이다.  
        if(pv == null)
            return;

        if(pv.IsMine == false)  //pv.IsMine이 아니면 리턴시켜서 pv.IsMine일때만 처리해 줄 계획임
            return;

        if (a_TakeHeroId < 0)
            return;

        if (0 <= m_TakeHeroId)  //누군가 이미 이 Item을 획득했다는 의미이므로...
            return;
        //로컬에서 한번 먹었으면 두번 먹지 못하게 하기 위한 의도

        m_TakeHeroId = a_TakeHeroId;

        Hero_Ctrl a_RefHero = null;
        GameObject[] a_Heros = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject a_Hero in a_Heros)
        {
            a_RefHero = a_Hero.GetComponent<Hero_Ctrl>();   
            if(a_RefHero.pv.Owner.ActorNumber == m_TakeHeroId)
            {
                a_RefHero.pv.RPC("TakeItemRPC",
                                    RpcTarget.AllViaServer, (int)m_ItemType);
                break;
            }
        }//foreach(GameObject a_Hero in a_Heros)

        PhotonNetwork.Destroy(this.gameObject);
        //즉시 삭제 중계 하는 것이 더 안전할 것으로 판단됨

    }//public void OnTrigItemRPC(int a_TakeHeroId) 

    void EnableOnOff(bool a_IsOn)
    {
        MeshRenderer[] a_MeshList = gameObject.GetComponentsInChildren<MeshRenderer>();
        for(int i = 0; i < a_MeshList.Length; i++)
        {
            if (a_MeshList[i] == null)
                continue;

            a_MeshList[i].enabled = a_IsOn;
        }//for(int i = 0; i < a_MeshList.Length; i++)

        BoxCollider[] a_BoxColls = gameObject.GetComponentsInChildren<BoxCollider>();   
        for(int i = 0; i < a_BoxColls.Length; i++)
        {
            if (a_BoxColls[i] == null)
                continue;

            a_BoxColls[i].enabled = a_IsOn;
        }//for(int i = 0; i < a_BoxColls.Length; i++)

    }//void EnableOnOff(bool a_IsOn)

}//public class ItemCtrl : MonoBehaviour
