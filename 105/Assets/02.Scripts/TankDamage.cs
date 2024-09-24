using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//hp ������ ����ȭ�� �ȸ´� ���� (�ذ��� : IsMine �������� �����ϰ� �߰��Ѵ�.)
//�����ϰ� �Ѿ� ���� �浹 ó���� �Ϸ��� ������ ��ũ(�ƹ�Ÿ ��ũ)�� ���, �Ѿ��� �¾��� ��
//IsMine���� �Ѿ��� �¾Ҵٴ� RPC �Լ��� ���
//(���⼭ RPC �Լ��� �Ѿ� ������ȣ�� ���� ������ �ߺ� �������� ���� �ʵ��� ó�� �ؾ� �Ѵ�.)
//IsMine���� Hp ������ ����� �� �� �� Hp�� �߰��Ͽ� ����ȭ�� ������� �Ѵ�.

public class TankDamage : MonoBehaviourPunCallbacks, IPunObservable
{
    //��ũ ���� �� ���� ó���� ���� MeshRenderer ������Ʈ �迭
    private MeshRenderer[] renderers;

    //��ũ ���� ȿ�� �������� ������ ����
    private GameObject expEffect = null;

    //��ũ�� �ʱ� ����ġ
    private int initHp = 200;
    //��ũ�� ���� ����ġ
    [HideInInspector] public int currHp = 200;
    int NetHp = 200;    //�ƹ�Ÿ ��ũ(������ ��ũ)���� Hp ���� ����ȭ �����ֱ� ���� ����
    //�ƹ�Ÿ ��ũ ���忡���� ��Ʈ������ Hp ���� �޾Ƽ� �״� ������ �ľ��ؼ� ���ó�� �ϱ� ����...

    //��ũ ������ Canvas ��ü�� ������ ����
    public Canvas hudCanvas;
    //Filled Ÿ���� ImageUI �׸��� ������ ����
    public Image hpBar;

    PhotonView pv = null;

    //--- Kill Count ����ȭ�� �ʿ��� ����
    //��ũ HUD�� ǥ���� ���ھ� Text UI �׸�
    public Text txtKillCount;

    //�÷��̾� ID(������ȣ) �����ϴ� ����
    [HideInInspector] public int PlayerId = -1;

    //�� ��ũ �ı� ���ھ CustomProperties�� ���� �߰��ϱ� ���� ������...
    int m_KillCount = 0;    //��� ��ǻ���� �� ��ũ���� ����
    int m_Cur_LAttId = -1;  //���� ������ ����(��Ÿ) �ߴ���? Last Attack Id

    ExitGames.Client.Photon.Hashtable KillProps = 
                                        new ExitGames.Client.Photon.Hashtable();
    //--- Kill Count ����ȭ�� �ʿ��� ����

    [HideInInspector] public float m_ReSetTime = 0.0f;  //��Ȱ�ð������� 
    //���� �����Ŀ��� ������ �ֱ� 10�ʵ���

    int m_StCount = 0;
    Vector3 m_StPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
        PlayerId = pv.Owner.ActorNumber;    //�÷��̾��� ������ȣ�� �ϳ� ������ ��

        //��ũ ���� ��� MeshRenderer ������Ʈ�� ������ �� �迭�� �Ҵ�
        renderers = GetComponentsInChildren<MeshRenderer>();

        //���� ����ġ�� �ʱ� ����ġ�� �ʱⰪ ����
        currHp = initHp;

        //��ũ ���� �� ������ų ���� ȿ���� �ε�
        expEffect = Resources.Load<GameObject>("ExplosionMobile");

        //Filled �̹��� ������ ������� ����
        hpBar.color = Color.green;

        //--- CustomProperties �ʱ�ȭ
        InitCustomProperties(pv);
        //--- CustomProperties �ʱ�ȭ
    }//void Start()

    int m_UpdateCk = 2;

    // Update is called once per frame
    void Update()
    {
        //��ũ�� �濡 ó�� �����ϸ� 
        //�켱 ��ũ�� ���¸� �ı��� ����ó��...
        //������ �ʰ� �ϰ� ��� Ready ���°� �Ǿ��� �� �����ϰ� �Ѵ�.
        //Ÿ�̹� �� ��� Update�� ���� �Ŀ� �����ؾ� UI�� ������ �ʴ´�.(�̻�����)
        if(0 < m_UpdateCk)
        {
            m_UpdateCk--;
            if(m_UpdateCk <= 0)
            {
                ReadyStateTank();
            }
        }
        //��ũ �濡 ó�� �����Ͻ� ó�� �κ� 

        if (0.0f < m_ReSetTime)
            m_ReSetTime -= Time.deltaTime;

        if (PhotonNetwork.CurrentRoom == null ||
            PhotonNetwork.LocalPlayer == null)
            return;
        //����ȭ ������ ���� �϶��� ������Ʈ�� ����� �ش�.

        //if (0 < m_StCount)
        //{
        //    this.transform.position = m_StPos;
        //    m_StCount--;
        //}

        //--- ���� �÷��̾�(�ƹ�Ÿ ��ũ ����) �� �� ����ȭ �ڵ�
        if (pv.IsMine == false)
        { //���� �÷��̾�(�ƹ�Ÿ ��ũ ����) �� �� ����
            AvataUpdate();

            ReceiveKillCount(); //�ƹ�Ÿ ��ũ�� ���忡�� KillCount �߰� ��ƿ�
        }
        //--- ���� �÷��̾�(�ƹ�Ÿ ��ũ ����) �϶� ����ȭ �ڵ�

        if (txtKillCount != null)
            txtKillCount.text = m_KillCount.ToString(); //ų ī��Ʈ UI ����
    }

    public void ReadyStateTank()
    {
        if (GameMgr.m_GameState != GameState.GS_Ready)
            return;

        StartCoroutine(this.WaitReadyTank());
    }

    //���� ���� ���...
    IEnumerator WaitReadyTank()
    {
        //HUD�� ��Ȱ��ȭ
        hudCanvas.enabled = false;

        //��ũ ���� ó��
        SetTankVisible(false);

        while(GameMgr.m_GameState == GameState.GS_Ready)
        {
            yield return null;
        }

        //��ũ Ư���� ��ġ�� ������ �ǵ���...
        float pos = Random.Range(-100.0f, 100.0f);
        Vector3 a_SitPos = new Vector3(pos, 20.0f, pos);

        string a_TeamKind = ReceiveSelTeam(pv.Owner);   //�ڱ� �Ҽ� �� �޾ƿ���
        int a_SitPosInx = ReceiveSitPosInx(pv.Owner);   //�ڱ� �ڸ� ��ȣ �޾ƿ���
        if(0 <= a_SitPosInx && a_SitPosInx < 4)
        {
            if(a_TeamKind == "blue")
            {
                a_SitPos = GameMgr.m_Team1Pos[a_SitPosInx];
                this.gameObject.transform.eulerAngles =
                                    new Vector3(0.0f, 201.0f, 0.0f);
            }
            else if(a_TeamKind == "black")
            {
                a_SitPos = GameMgr.m_Team2Pos[a_SitPosInx];
                this.gameObject.transform.eulerAngles =
                                    new Vector3(0.0f, 19.5f, 0.0f);
            }
        }//if(0 <= a_SitPosInx && a_SitPosInx < 4)

        this.gameObject.transform.position = a_SitPos;

        //��ũ Ư���� ��ġ�� ������ �ǵ���...

        //Filled �̹��� �ʱⰪ���� ȯ��
        hpBar.fillAmount = 1.0f;
        //Filled �̹��� ������ ������� ����
        hpBar.color = Color.green;
        //HUD Ȱ��ȭ
        hudCanvas.enabled = true;

        if (pv != null && pv.IsMine == true)    //������ �� ���� �ʱⰪ ����
            currHp = initHp;

        //��ũ�� �ٽ� ���̰� ó��
        SetTankVisible(true);

        m_StPos = a_SitPos;
        m_StCount = 5;

    }//IEnumerator WaitReadyTank()

    void OnTriggerEnter(Collider coll)
    {
        //�浹�� Collider�� ��ũ ��
        if(currHp > 0 && coll.tag == "CANNON")
        {
            int a_Att_Id = -1;
            string a_AttTeam = "blue";
            Cannon a_RefCannon = coll.gameObject.GetComponent<Cannon>();
            if (a_RefCannon != null)
            {
                a_Att_Id = a_RefCannon.AttackerId;
                a_AttTeam = a_RefCannon.AttackerTeam;
            }

            TakeDamage(a_Att_Id, a_AttTeam);

            //currHp -= 20;

            ////���� ����ġ ����� = (���� ����ġ) / (�ʱ� ����ġ)
            //hpBar.fillAmount = (float)currHp / (float)initHp;

            ////���� ��ġ�� ���� Filled �̹����� ������ ����
            //if(hpBar.fillAmount <= 0.4f)
            //    hpBar.color = Color.red;
            //else if(hpBar.fillAmount <= 0.6f)
            //    hpBar.color = Color.yellow;

            //if(currHp <= 0)
            //{
            //    StartCoroutine(this.ExplosionTank());
            //}
        }
    }

    public void TakeDamage(int AttackerId = -1, string a_AttTeam = "blue")
    {
        //�ڱⰡ �� �Ѿ��� �ڽ��� ������ �ȵǵ��� ó��...
        if (AttackerId == PlayerId)
            return;

        if (currHp <= 0.0f)
            return;

        //�ǰݿ���

        if (pv.IsMine == false) //������ ��ũ(�ƹ�Ÿ)�� ����
            return;

        if (0.0f < m_ReSetTime) //���� ���� �� 10�� ���� ������ �ֱ�
            return;

        string a_DamageTeam = "blue";
        if (pv.Owner.CustomProperties.ContainsKey("MyTeam") == true)
            a_DamageTeam = (string)pv.Owner.CustomProperties["MyTeam"];

        //���� �������� �޴� ��ũ�� AttackerId ������ ���� 
        //�ٸ� ���϶��� �������� ������ ó��
        if (a_AttTeam == a_DamageTeam)
            return;

        //pv.IsMine �� ��
        m_Cur_LAttId = AttackerId;
        currHp -= 20;
        if (currHp < 0)
            currHp = 0;

        //���� ����ġ ����� = (���� ����ġ) / (�ʱ� ����ġ)
        hpBar.fillAmount = (float)currHp / (float)initHp;

        //���� ��ġ�� ���� Filled �̹����� ������ ����
        if (hpBar.fillAmount <= 0.4f)
            hpBar.color = Color.red;
        else if (hpBar.fillAmount <= 0.6f)
            hpBar.color = Color.yellow;
        else
            hpBar.color = Color.green;

        if (currHp <= 0)  //�״� ó�� (�ƹ�Ÿ ��ũ���� �߰� �޾Ƽ� ó��)
        {
            //IsMine ���ؿ��� �״� ó��
            StartCoroutine(this.ExplosionTank());
        }
    }

    //���� ȿ�� ���� �� ������ �ڷ�ƾ �Լ�
    IEnumerator ExplosionTank()
    {
        //���� ȿ�� ����
        GameObject effect = GameObject.Instantiate(expEffect,
                                            transform.position, 
                                            Quaternion.identity);
        Destroy(effect, 3.0f);

        //HUD�� ��Ȱ��ȭ
        hudCanvas.enabled = false;

        //��ũ ���� ó��
        SetTankVisible(false);

        yield return null;

        //if (pv != null && pv.IsMine == true)  //�ǻ츮�� �� IsMine �϶��� �ϰ�
        //{
        //    //10�� ���� ��ٷȴٰ� Ȱ��ȭ�ϴ� ������ ����
        //    yield return new WaitForSeconds(10.0f);

        //    //Filled �̹��� �ʱ갪���� ȯ��
        //    hpBar.fillAmount = 1.0f;
        //    //Filled �̹��� ������ ������� ����
        //    hpBar.color = Color.green;
        //    //HUD Ȱ��ȭ
        //    hudCanvas.enabled = true;

        //    //������ �� ���� �ʱ갪 ����
        //    currHp = initHp;
        //    //��ũ�� �ٽ� ���̰� ó��
        //    SetTankVisible(true);
        //}
        //else
        //{
        //    //�ƹ�Ÿ ��ũ���� ���� �߰� �޾Ƽ� �ǻ츮�ڴٴ� ��
        //    yield return null;  //�� �÷��� ������ ���� �ڷ�ƾ �Լ� ������ �����Ѵ�.
        //}

    }//IEnumerator ExplosionTank()

    //MeshRenderer�� Ȱ��ȭ/��Ȱ��ȭ�ϴ� �Լ�
    void SetTankVisible(bool isVisible)
    {
        foreach(MeshRenderer _renderer in renderers)
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

        if (isVisible == true)
            m_ReSetTime = 10.0f;

    }//void SetTankVisible(bool isVisible)

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)  //IsMine�� ���� �۽�
        {
            stream.SendNext(m_Cur_LAttId); //�̹��� -20 ���� �� ���� �������� ���� �����شٴ� �ǵ�
            stream.SendNext(currHp);
        }
        else //���� �÷��̾�(�ƹ�Ÿ)�� ���� ����
        {
            m_Cur_LAttId = (int)stream.ReceiveNext(); //�ƹ�Ÿ�鵵 ���� ������ hp�� ���� ���� �˰� �Ѵ�.
            NetHp = (int)stream.ReceiveNext();
            //�ƹ�Ÿ ���忡�� ��� ������ �˱� ���� NetHp ��� ������ ���� ����
            //IsMine���� �۽��� �� Hp ���� �޾Ҵ�.
        }
    }// public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)

    void AvataUpdate()  //������ �÷��̾� Hp Update ó�� �Լ�
    {
        if(0 < currHp)
        {
            currHp = NetHp;

            //���� ����ġ ����� = (���� ����ġ) / (�ʱ� ����ġ)
            hpBar.fillAmount = (float)currHp / (float)initHp;

            //���� ��ġ�� ���� Filled �̹����� ������ ����
            if (hpBar.fillAmount <= 0.4f)
                hpBar.color = Color.red;
            else if (hpBar.fillAmount <= 0.6f)
                hpBar.color = Color.yellow;
            else
                hpBar.color = Color.green;

            if(currHp <= 0) //�״� ó�� (�ƹ�Ÿ ��ũ���� �߰� �޾Ƽ� ó��)
            {
                currHp = 0;

                if(0 <= m_Cur_LAttId)  //������ Id�� ��ȿ�� ��
                { //���� Hp�� ���̰� �ؼ� ����� �̸��� �� ��ũ�� ��������?
                    // ���� ���� ��ũ ���忡����
                    // �������� AttackerId (<--- IsMine)�� ã������ 
                    // ���� ��ũ �ƹ�Ÿ�� �߿��� AttackerId (<--- IsMine)�� ã�Ƽ�
                    // KillCount�� �������� ��� �Ѵ�.
                    // �ڽ��� �ı���Ų �� ��ũ�� ���ھ ������Ű�� �Լ��� ȣ��
                    SaveKillCount(m_Cur_LAttId);
                }

                //IsMine ���ؿ��� �״� ó��
                StartCoroutine(this.ExplosionTank());
            }

        } //if(0 < currHp)
        else //if(currHp <= 0) �׾� �ִ� ��Ȳ����
        { //�׾� ���� �� ��� NetHp�� 0���� ��� ������ �ǰ�
            //�ǻ���� �ϴ� ��Ȳ ó��
            currHp = NetHp;
            if((int)(initHp * 0.95f) < currHp) //�̹��� ���� Hp�� �ִ� �������� ������
            {   //�ǻ���� �ϴ� ��Ȳ���� �Ǵ��ϰڴٴ� ��

                //Filled �̹��� �ʱⰪ���� ȯ��
                hpBar.fillAmount = 1.0f;
                //Filled �̹��� ������ ������� ����
                hpBar.color = Color.green;
                //HUD Ȱ��ȭ
                hudCanvas.enabled = true;

                //������ �� �� ���� �ʱⰪ ����
                currHp = initHp;
                //��ũ�� �ٽ� ���̰� ó��
                SetTankVisible(true);

            }//if ((int)(initHp * 0.95f) < currHp) //�̹��� ���� Hp�� �ִ� �������� ������
        }//else //if(currHp <= 0) �׾� �ִ� ��Ȳ����

    }//void AvataUpdate()  //������ �÷��̾� Hp Update ó�� �Լ�

    //�ڽ��� �ı���Ų �� ��ũ�� �˻��� ���ھ ������Ű�� �Լ�
    void SaveKillCount(int AttacketId)
    {
        //TANL ��ũ�� ������ ��� ��ũ�� ������ �迭�� ����
        GameObject[] tanks = GameObject.FindGameObjectsWithTag("TANK");
        foreach(GameObject tank in tanks)
        {
            var tankDamage = tank.GetComponent<TankDamage>();
            if(tankDamage != null && tankDamage.PlayerId == AttacketId)
            { //��ũ�� PlayerId�� ��ź�� AttackerId�� �������� �Ǵ�
                if(tankDamage.IncKillCount() == true)
                {
                    return;
                }
            }//if(tankDamage != null && tankDamage.PlayerId == AttacketId)
        }//foreach(GameObject tank in tanks)
    }// void SaveKillCount(int AttacketId)

    public bool IncKillCount() //���� ��ũ IsMine ���忡�� �� �Լ��� ȣ��Ǿ�� �Ѵ�.
    {
        if(pv != null && pv.IsMine == true)
        {
            //IsMine �ѱ������� KillCount�� ������Ű�� �̷���
            //IsMine�� ������Ű�� �ƹ�Ÿ�� KillCount�� �������� �߰��ϴٺ���
            //KillCount�� ��߳� �� �ֱ� �����̴�.
            m_KillCount++;

            //IsMone�϶��� ��ε� ĳ����(�߰�) <-- CustormProperties������ �̰� �� ��� �߰� �ȴ�. 
            SendKillCount(m_KillCount);

            return true;
        }

        return false;
    }

    void InitCustomProperties(PhotonView pv)
    {  //���۸� �̸� ����� ���� ���� �Լ�
        if(pv != null && pv.IsMine == true)
        { //pv.IsMine == true ���� �����ϰ� �ִ� ��ũ�̰� ����������...
            KillProps.Clear();
            KillProps.Add("KillCount", 0);
            pv.Owner.SetCustomProperties(KillProps);
        }
    }//void InitCustomProperties(PhotonView pv)

    void SendKillCount(int a_KillCount = 0)
    {
        if (pv == null)
            return;

        if (pv.IsMine == false) //IsMine �� ���� ���� �ϰڴٴ� ��
            return;

        if (KillProps == null)
        {
            KillProps = new ExitGames.Client.Photon.Hashtable();
            KillProps.Clear();
        }

        if (KillProps.ContainsKey("KillCount") == true)
            KillProps["KillCount"] = a_KillCount;
        else
            KillProps.Add("KillCount", a_KillCount);

        pv.Owner.SetCustomProperties(KillProps);

    }//void SendKillCount(int a_KillCount = 0)

    void ReceiveKillCount() //������ ��ũ���� KillCount �޾Ƽ� ó���ϴ� �κ�
    {
        if(pv == null)
            return;

        if(pv.IsMine == true)   //������ ��ũ�鸸 �ޱ� ���ؼ�...
            return;

        if(pv.Owner == null)
            return;

        if(pv.Owner.CustomProperties.ContainsKey("KillCount") == true)
        {
            m_KillCount = (int)pv.Owner.CustomProperties["KillCount"];
        }
    }//void ReceiveKillCount() 

    string ReceiveSelTeam(Player a_Player) //using Photon.Realtime;
    {
        string a_TeamKind = "blue";

        if (a_Player == null)
            return a_TeamKind;

        if (a_Player.CustomProperties.ContainsKey("MyTeam") == true)
            a_TeamKind = (string)a_Player.CustomProperties["MyTeam"];

        return a_TeamKind;
    }

    int ReceiveSitPosInx(Player a_Player)
    {
        int a_SitIdx = -1;

        if (a_Player == null)
            return a_SitIdx;

        if (a_Player.CustomProperties.ContainsKey("SitPosInx") == true)
            a_SitIdx = (int)a_Player.CustomProperties["SitPosInx"];

        return a_SitIdx;
    }
}
