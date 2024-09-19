using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//hp 에너지 동기화가 안맞는 현상 (해결방법 : IsMine 기준으로 판정하고 중계한다.)
//정밀하게 총알 원격 충돌 처리를 하려면 원격지 탱크(아바타 탱크)인 경우, 총알을 맞았을 때
//IsMine으로 총알을 맞았다는 RPC 함수를 쏘고
//(여기서 RPC 함수에 총알 고유번호를 같이 보내서 중복 데미지가 들어가지 않도록 처리 해야 한다.)
//IsMine에서 Hp 데미지 계산을 해 준 후 Hp를 중계하여 동기화를 맞춰줘야 한다.

public class TankDamage : MonoBehaviourPunCallbacks, IPunObservable
{
    private MeshRenderer[] renderers;
    private GameObject expEffect = null;
    private int initHp = 200;
    [HideInInspector] public int currHp = 200;
    int NetHp = 200;
    public Canvas hudCanvas;
    public Image hpBar;
    PhotonView pv = null;

    #region KillCount 동기화
    public Text TxtKillCnt;
    [HideInInspector] public int PlayerId = -1;
    int m_KillCnt = 0;
    int m_Cur_LAttId = -1;
    ExitGames.Client.Photon.Hashtable m_KillProps = new ExitGames.Client.Photon.Hashtable();
    #endregion

    void Start()
    {
        pv = GetComponent<PhotonView>();
        PlayerId = pv.Owner.ActorNumber;
        renderers = GetComponentsInChildren<MeshRenderer>();
        currHp = initHp;
        expEffect = Resources.Load<GameObject>("ExplosionMobile");
        hpBar.color = Color.green;
        InitCustomProperties(pv);
    }


    int m_UpCk = 2;

    void Update()
    {

        //방 처음 입장 시 상태 파괴된 이후 처럼 보이지않게 모두 Ready상태가 되면 시작
        if(0 < m_UpCk)
        {
            m_UpCk--;
            if (m_UpCk <= 0)
            {
                ReayStTank();
            }

        }




        if (PhotonNetwork.CurrentRoom == null
            || PhotonNetwork.LocalPlayer == null)
            return;

        if (pv.IsMine == false)
        {
            AvataUpdate();
        }

        ReceiveKillCnt();

        if (TxtKillCnt != null)
        {
            TxtKillCnt.text = m_KillCnt.ToString();
        }
    }

    void ReayStTank()
    {
        if(GameMgr.GState != GameState.G_Ready) return;

        StartCoroutine(this.WReadyTank());
    }

    IEnumerator WReadyTank()
    {
        hudCanvas.enabled = false;

        SetTankVisible(false);

        while(GameMgr.GState == GameState.G_Ready)
        {
            yield return null;
        }
    }


    void OnTriggerEnter(Collider coll)
    {
        if (currHp > 0 && coll.tag == "CANNON")
        {
            int a_Att_Id = -1;
            Cannon a_RefCannon = coll.gameObject.GetComponent<Cannon>();
            if (a_RefCannon != null)
                a_Att_Id = a_RefCannon.AttackerId;

            TakeDamage(a_Att_Id);
        }
    }

    public void TakeDamage(int AttackerId = -1)
    {
        if (AttackerId == PlayerId)
            return;

        if (currHp <= 0.0f)
            return;

        if (pv.IsMine == false)
            return;

        m_Cur_LAttId = AttackerId;
        currHp -= 20;
        if (currHp < 0)
            currHp = 0;

        hpBar.fillAmount = (float)currHp / (float)initHp;

        if (hpBar.fillAmount <= 0.4f)
            hpBar.color = Color.red;
        else if (hpBar.fillAmount <= 0.6f)
            hpBar.color = Color.yellow;
        else
            hpBar.color = Color.green;

        if (currHp <= 0)
        {
            StartCoroutine(this.ExplosionTank());
        }
    }

    IEnumerator ExplosionTank()
    {
        GameObject effect = GameObject.Instantiate(expEffect, transform.position, Quaternion.identity);
        Destroy(effect, 3.0f);
        hudCanvas.enabled = false;
        SetTankVisible(false);

        if (pv != null && pv.IsMine == true)
        {
            yield return new WaitForSeconds(10.0f);
            hpBar.fillAmount = 1.0f;
            hpBar.color = Color.green;
            hudCanvas.enabled = true;
            currHp = initHp;
            SetTankVisible(true);
        }
        else
        {
            yield return null;
        }
    }

    void SetTankVisible(bool isVisible)
    {
        foreach (MeshRenderer _renderer in renderers)
        {
            _renderer.enabled = isVisible;
        }

        Rigidbody[] a_Rigs = GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody _Rigd in a_Rigs)
        {
            _Rigd.isKinematic = !isVisible;
        }

        BoxCollider[] a_BoxColls = this.GetComponentsInChildren<BoxCollider>(true);
        foreach (BoxCollider _BoxColl in a_BoxColls)
        {
            _BoxColl.enabled = isVisible;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_Cur_LAttId);
            stream.SendNext(currHp);
        }
        else
        {
            m_Cur_LAttId = (int)stream.ReceiveNext();
            NetHp = (int)stream.ReceiveNext();
        }
    }

    void AvataUpdate()
    {
        if (0 < currHp)
        {
            currHp = NetHp;
            hpBar.fillAmount = (float)currHp / (float)initHp;

            if (hpBar.fillAmount <= 0.4f)
                hpBar.color = Color.red;
            else if (hpBar.fillAmount <= 0.6f)
                hpBar.color = Color.yellow;
            else
                hpBar.color = Color.green;

            if (currHp <= 0)
            {
                currHp = 0;

                if (0 <= m_Cur_LAttId)
                {
                    SaveKillCnt(m_Cur_LAttId);
                }

                StartCoroutine(this.ExplosionTank());
            }
        }
        else
        {
            currHp = NetHp;
            if ((int)(initHp * 0.95f) < currHp)
            {
                hpBar.fillAmount = 1.0f;
                hpBar.color = Color.green;
                hudCanvas.enabled = true;
                currHp = initHp;
                SetTankVisible(true);
            }
        }
    }

    void SaveKillCnt(int AttId)
    {
        GameObject[] a_Tanks = GameObject.FindGameObjectsWithTag("Tank");

        foreach (GameObject a_Obj in a_Tanks)
        {
            var a_TankDamage = a_Obj.GetComponent<TankDamage>();

            if (a_TankDamage != null && a_TankDamage.PlayerId == AttId)
            {
                if (a_TankDamage.IncKillCnt() == true)
                {
                    break;
                }
            }
        }
    }

    public bool IncKillCnt()
    {
        if (pv != null && pv.IsMine == true)
        {
            m_KillCnt++;
            SendKillCnt(m_KillCnt);
            return true;
        }
        return false;
    }

    void InitCustomProperties(PhotonView pv)
    {
        if (pv != null && pv.IsMine == true)
        {
            m_KillProps.Clear();
            m_KillProps.Add("KillCount", 0);
            pv.Owner.SetCustomProperties(m_KillProps);
        }
    }

    void SendKillCnt(int a_Killcnt = 0)
    {
        if (pv == null) return;
        if (pv.IsMine == false) return;

        if (m_KillProps == null)
        {
            m_KillProps = new ExitGames.Client.Photon.Hashtable();
            m_KillProps.Clear();
        }

        if (m_KillProps.ContainsKey("KillCount") == true)
        {
            m_KillProps["KillCount"] = a_Killcnt;
        }
        else
        {
            m_KillProps.Add("KillCount", a_Killcnt);
        }

        pv.Owner.SetCustomProperties(m_KillProps);
    }

    void ReceiveKillCnt()
    {
        if (pv == null) return;
        if (pv.IsMine == true) return;
        if (pv.Owner == null) return;

        if (pv.Owner.CustomProperties.ContainsKey("KillCount") == true)
        {
            m_KillCnt = (int)pv.Owner.CustomProperties["KillCount"];
        }
    }
}
