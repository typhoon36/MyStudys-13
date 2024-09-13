using Photon.Pun;
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

    //�÷��̾� ID(������ȣ) �����ϴ� ����
    [HideInInspector] public int PlayerId = -1;

    //�� ��ũ �ı� ���ھ CustomProperties�� ���� �߰��ϱ� ���� ������...
    int m_KillCount = 0;    //��� ��ǻ���� �� ��ũ���� ����
    int m_Cur_LAttId = -1;  //���� ������ ����(��Ÿ) �ߴ���? Last Attack Id

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
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.CurrentRoom == null ||
            PhotonNetwork.LocalPlayer == null)
            return;
        //����ȭ ������ ���� �϶��� ������Ʈ�� ����� �ش�.

        //--- ���� �÷��̾�(�ƹ�Ÿ ��ũ ����) �� �� ����ȭ �ڵ�
        if (pv.IsMine == false)
        { //���� �÷��̾�(�ƹ�Ÿ ��ũ ����) �� �� ����
            AvataUpdate();
        }
        //--- ���� �÷��̾�(�ƹ�Ÿ ��ũ ����) �϶� ����ȭ �ڵ�
    }

    void OnTriggerEnter(Collider coll)
    {
        //�浹�� Collider�� ��ũ ��
        if(currHp > 0 && coll.tag == "CANNON")
        {
            int a_Att_Id = -1;
            Cannon a_RefCannon = coll.gameObject.GetComponent<Cannon>();
            if (a_RefCannon != null)
                a_Att_Id = a_RefCannon.AttackerId;

            TakeDamage(a_Att_Id);

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

    public void TakeDamage(int AttackerId = -1)
    {
        //�ڱⰡ �� �Ѿ��� �ڽ��� ������ �ȵǵ��� ó��...
        if (AttackerId == PlayerId)
            return;

        if (currHp <= 0.0f)
            return;

        //�ǰݿ���

        if (pv.IsMine == false) //������ ��ũ(�ƹ�Ÿ)�� ����
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

        if (pv != null && pv.IsMine == true)  //�ǻ츮�� �� IsMine �϶��� �ϰ�
        {
            //10�� ���� ��ٷȴٰ� Ȱ��ȭ�ϴ� ������ ����
            yield return new WaitForSeconds(10.0f);

            //Filled �̹��� �ʱ갪���� ȯ��
            hpBar.fillAmount = 1.0f;
            //Filled �̹��� ������ ������� ����
            hpBar.color = Color.green;
            //HUD Ȱ��ȭ
            hudCanvas.enabled = true;

            //������ �� ���� �ʱ갪 ����
            currHp = initHp;
            //��ũ�� �ٽ� ���̰� ó��
            SetTankVisible(true);
        }
        else
        {
            //�ƹ�Ÿ ��ũ���� ���� �߰� �޾Ƽ� �ǻ츮�ڴٴ� ��
            yield return null;  //�� �÷��� ������ ���� �ڷ�ƾ �Լ� ������ �����Ѵ�.
        }
    }

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
                    // �ƹ�Ÿ�� �߿��� AttackerId (<--- IsMine)�� ã�Ƽ�
                    // KillCount�� �������� ��� �Ѵ�.
                    // �ڽ��� �ı���Ų �� ��ũ�� ���ھ ������Ű�� �Լ��� ȣ��

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
}
