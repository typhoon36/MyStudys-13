using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCannon : MonoBehaviour
{
    //Cannon �������� ������ ����
    public GameObject cannon = null;
    //��ź �߻� ���� ����
    private AudioClip fireSfx = null;
    //AudioSource ������Ʈ�� �Ҵ��� ����
    private AudioSource sfx = null;

    //cannon �߻� ����
    public Transform firePos;

    //PhotonView ������Ʈ�� �Ҵ��� ����
    private PhotonView pv = null;

    TankDamage m_TankDamage = null;

    void Awake()
    {
        //cannon �������� Resources �������� �ҷ��� ������ �Ҵ�
        cannon = (GameObject)Resources.Load("Cannon");
        //��ź �߻� ���� ������ Resources �������� �ҷ��� ������ �Ҵ�
        fireSfx = Resources.Load<AudioClip>("CannonFire");
        //AutioSource ������Ʈ�� �Ҵ�
        sfx = GetComponent<AudioSource>();  
        //PhotonView ������Ʈ�� pv ������ �Ҵ�
        pv = GetComponent<PhotonView>();    
    }

    // Start is called before the first frame update
    void Start()
    {
        m_TankDamage = GetComponent<TankDamage>();  
    }

    // Update is called once per frame
    void Update()
    {
        //PhotonView�� �ڽ��� ���̰�, ���콺 ���� ��ư Ŭ�� �� �߻� ���� ����
        if(pv.IsMine && Input.GetMouseButtonDown(0))
        {
            //���콺�� UI ���� ������...
            if (GameMgr.IsPointerOverUIObject() == true)
                return;

            if (m_TankDamage != null && m_TankDamage.currHp <= 0) //�׾� ������ �� ���...
                return;

            if(GameMgr.GState != GameState.G_Play)return;

            //�ڽ��� ��ũ�� ���� �����Լ��� ȣ���� ��ź�� �߻�
            Fire();
            //���� ��Ʈ��ũ �÷��̾��� ��ũ�� RPC�� �������� Fire �Լ��� ȣ��
            pv.RPC("Fire", RpcTarget.Others, null);
        }
    }

    [PunRPC]
    void Fire()
    {
        //�߻� ���� �߻�
        sfx.PlayOneShot(fireSfx, 0.2f);
        GameObject a_Cannon = Instantiate(cannon, firePos.position, firePos.rotation);
        a_Cannon.GetComponent<Cannon>().AttackerId = pv.Owner.ActorNumber;
        //Owner : ������
        //�ƹ�Ÿ ��ũ�� ���忡���� ���� �� ��ũ�� �����ڰ� ������?
        //�� ��ũ�� IsMine�� ��������� ������ �� �ִ� ���
    }
}
