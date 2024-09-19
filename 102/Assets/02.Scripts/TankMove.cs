using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityStandardAssets.Utility;

public class TankMove : MonoBehaviourPunCallbacks, IPunObservable
{
    //��ũ�� �̵� �� ȸ�� �ӵ��� ��Ÿ���� ����
    public float moveSpeed = 20.0f;
    public float rotSpeed = 50.0f;

    //������ ������Ʈ�� �Ҵ��� ����
    private Rigidbody rbody;
    //Ű���� �Է°� ����
    private float h, v;

    //PhotonView ������Ʈ�� �Ҵ��� ����
    private PhotonView pv = null;
    //���� ī�޶� ������ CamPivot ���ӿ�����Ʈ
    public Transform camPivot;

    //��ġ ������ �ۼ����� �� ����� ���� ���� �� �ʱ갪 ����
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    //--- ��ũ�� �浹 ���� ó���� ���� ����
    TankDamage m_TankDamage = null;
    Terrain m_RefMap = null;
    Vector3 m_CacPos = Vector3.zero;
    //--- ��ũ�� �浹 ���� ó���� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        //������Ʈ �Ҵ�
        rbody = GetComponent<Rigidbody>();

        //Rigidbody�� �����߽��� ���� ����
        rbody.centerOfMass = new Vector3(0.0f, -2.5f, 0.0f);

        //PhotonView ������Ʈ �Ҵ�
        pv = GetComponent<PhotonView>();

        //PhotonView�� �ڽ��� ��ũ�� ���
        if(pv.IsMine)
        {
            //���� ī�޶� �߰��� SmoothFollow ��ũ��Ʈ�� ���� ����� ����
            Camera.main.GetComponent<SmoothFollow>().target = camPivot;
        }
        else
        {
            //���� ��Ʈ��ũ �÷��̾��� ��ũ(�ƹ�Ÿ)�� �������� �̿����� ����
            rbody.isKinematic = true;
        }

        //���� ��ũ�� ��ġ �� ȸ�� ���� ó���� ������ �ʱⰪ ����
        currPos = transform.position;
        currRot = transform.rotation;

        //--- ��ũ�� �浹 ���� ó���� ���� ����
        m_TankDamage = GetComponent<TankDamage>();
        m_RefMap = FindObjectOfType<Terrain>();
        //--- ��ũ�� �浹 ���� ó���� ���� ����

    }//void Start()

    // Update is called once per frame
    void Update()
    {
        //�ڽ��� ���� ��Ʈ��ũ ���ӿ�����Ʈ�� �ƴ� ���� Ű���� ���� ��ƾ�� ����
        if (pv.IsMine)
        {
            if (GameMgr.bEnter == true)
                return;

            if(GameMgr.GState != GameState.G_Play)
                return;

            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            //ȸ���� �̵� ó��
            transform.Rotate(Vector3.up * rotSpeed * h * Time.deltaTime);
            transform.Translate(Vector3.forward * v * moveSpeed * Time.deltaTime);
            //rbody.angularVelocity = Vector3.up * rotSpeed * h;
            //Vector3 a_Move = Vector3.forward * v * moveSpeed;
            //rbody.velocity = new Vector3(a_Move.x, rbody.velocity.y, a_Move.z);

            UnitCollUpdate();
        }
        else  //���� �÷��̾�(�ƹ�Ÿ)�� �� ����
        {
            if (10.0f < (transform.position - currPos).magnitude)
            {
                //�߰� ���� ��ǥ�� ������ǥ�� �Ÿ� ���� 10m �̻��̸� ��� ���� ����
                transform.position = currPos;
            }
            else
            {
                //���� �÷��̾��� ��ũ(�ƹ�Ÿ)�� ���Ź��� ��ġ���� �ε巴�� �̵���Ŵ
                transform.position = Vector3.Lerp(transform.position, currPos, Time.deltaTime * 10.0f);
            }

            //���� �÷��̾��� ��ũ(�ƹ�Ÿ)�� ���Ź��� ������ŭ �ε巴�� ȸ����Ŵ
            transform.rotation = Quaternion.Slerp(transform.rotation, currRot, Time.deltaTime * 10.0f);
        }//else  //���� �÷��̾�(�ƹ�Ÿ)�� �� ����

    } //void Update()

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //���� �÷��̾�(IsMine)�� ��ġ ���� �۽�
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else //���� �÷��̾�(�ƹ�Ÿ)�� ��ġ ���� ����
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
        }
    }

    void UnitCollUpdate()
    {
        if (m_TankDamage == null)
            return;

        if (m_TankDamage.currHp <= 0.0f)
            return;

        if (m_RefMap == null)
            return;

        //--- ���� ������ ������ ��...
        float a_CurHeight = m_RefMap.SampleHeight(transform.position);
        if(transform.position.y < (a_CurHeight - 1.0f))
        {
            transform.position = new Vector3(transform.position.x, 
                                        a_CurHeight + 1.0f, transform.position.z);
            if(rbody != null)
            {
                rbody.velocity = new Vector3(0, rbody.velocity.y, 0);
                rbody.angularVelocity = new Vector3(0, 0, 0);
            }
        }
        //--- ���� ������ ������ ��...

        //--- ��ũ�� ���� ������ ����� ���ϰ� ����...
        m_CacPos = transform.position;
        if (245.0f < transform.position.x)
            m_CacPos.x = 245.0f;
        if (245.0f < transform.position.z)
            m_CacPos.z = 245.0f;
        if (transform.position.x < -245.0f)
            m_CacPos.x = -245.0f;
        if (transform.position.z < -245.0f)
            m_CacPos.z = -245.0f;
        transform.position = m_CacPos;
        //--- ��ũ�� ���� ������ ����� ���ϰ� ����...

        //--- ��ũ�� ������ ������ �ϸ� �ٷ� �����ش�.
        if(transform.position.y < (a_CurHeight + 8.0f))
        if(Vector3.Dot(transform.up, Vector3.up) <= 0.2f)
        {
            transform.up = Vector3.up;
            rbody.angularVelocity = new Vector3(0, 0, 0);
        }
        //--- ��ũ�� ������ ������ �ϸ� �ٷ� �����ش�.

    }//void UnitCollUpdate()
}
