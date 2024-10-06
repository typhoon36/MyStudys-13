using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPos   //List�� �����ϰ� List�̱� ������ �ε����� ���� �ȴ�.
{
    public Vector3 m_Pos = Vector3.zero;
    public Quaternion m_Rot = Quaternion.identity;
    public float m_SpawnTime = 3.0f;

    public SpawnPos(Vector3 a_Pos, Quaternion a_Rot)
    {
        m_Pos = a_Pos;
        m_Rot = a_Rot;
    }
}

// ������ ��ġ������ ������ Ŭ���̾�Ʈ�� �����Ѵ�.
// �׷��� InstantiateRoomObject �� ��������� �ƴ϶��
// ������ Ŭ���̾�Ʈ�� �����ٰ� ������ ���������� �μ��ΰ� �ǰ� �ϰ� 
// �ͱ� ������ �� Ŭ������ InstantiateRoomObject �� ����� �ִ� �� ���ٰ� �Ǵ���

public class MonSpawn_Mgr : MonoBehaviourPunCallbacks, IPunObservable
{
    PhotonView pv = null;
    List<SpawnPos> m_SpawnPos = new List<SpawnPos>();

    float g_NetDelay = 0.0f;

    //--- �̱��� ����
    public static MonSpawn_Mgr Inst = null;

    void Awake()
    {
        Inst = this; 

        pv = GetComponent<PhotonView>();
    }
    //--- �̱��� ����

    // Start is called before the first frame update
    void Start()
    {
        Transform[] a_SPTList = gameObject.GetComponentsInChildren<Transform>();
        if(a_SPTList != null)
        {
            SpawnPos a_SPos = null;
            for(int i = 0; i < a_SPTList.Length; i++)
            {
                if (a_SPTList[i].name.Contains("MonSpawn_Mgr") == true)
                    continue;

                a_SPos = new SpawnPos(a_SPTList[i].position, a_SPTList[i].rotation);
                a_SPos.m_SpawnTime = Random.Range(1.5f, 5.0f);  //1.5�ʿ��� 5.0�� �ڿ� �����ǰ� �� �ش�.
                m_SpawnPos.Add(a_SPos);
            }//for(int i = 0; i < a_SPTList.Length; i++)
        }//if(a_SPTList != null)
    }//void Start()

    // Update is called once per frame
    void Update()
    {
        MonSpawnUpdate();
    }

    void MonSpawnUpdate()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;     //����ȭ ������ ���� �϶��� ������Ʈ�� ����� �ش�.

        if (PhotonNetwork.IsMasterClient == false)
            return;     //������ Ŭ���̾�Ʈ�� �ƴϸ� ������ ���� ���� ���ϰ� �Ѵ�.

        if (0.0f < g_NetDelay)
            g_NetDelay -= Time.deltaTime;

        if (0.0f < g_NetDelay)
            return;    //������ ����� ���� ��� ������ �ֱ� ������ ���� ����ȭ ����...

        if (m_SpawnPos.Count <= 0)
            return;

        GameObject TempMon = null;
        for(int i = 0; i < m_SpawnPos.Count; i++)
        {
            if (m_SpawnPos[i].m_SpawnTime <= 0)
                continue;

            m_SpawnPos[i].m_SpawnTime -= Time.deltaTime;
            if (m_SpawnPos[i].m_SpawnTime <= 0.0f)
            {
                int MonSterKind = Random.Range(0, 2);

                if(MonSterKind == 0)
                {
                    TempMon = PhotonNetwork.InstantiateRoomObject("Skeleton_Root",
                                                    m_SpawnPos[i].m_Pos, m_SpawnPos[i].m_Rot, 0);
                }
                else
                {
                    TempMon = PhotonNetwork.InstantiateRoomObject("Alien_Root",
                                                    m_SpawnPos[i].m_Pos, m_SpawnPos[i].m_Rot, 0);
                }

                if (TempMon != null)
                    TempMon.GetComponent<MonsterCtrl>().m_SpawnIdx = i;

            }//if (m_SpawnPos[i].m_SpawnTime <= 0.0f)

        }//for(int i = 0; i < m_SpawnPos.Count; i++)

    }//void MonSpawnUpdate()

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)  //pv.IsMine �� PhotonNetwork.IsMasterClient �̴�.
        {
            for(int i = 0; i < m_SpawnPos.Count; i++)
            {
                stream.SendNext(m_SpawnPos[i].m_SpawnTime);
            }
        }
        else
        {
            // PhotonNetwork.IsMasterClient �� ���� �����鼭...
            // pv.IsMine �� PhotonNetwork.IsMasterClient �� �ٲ� m_SpawnTime�� 
            // �μ� �ΰ� �ޱ� ���Ͽ�... �̸� �ٸ� ��ǻ�͵鵵 ��� �޾� ���´�.
            // �� �κ��� ���ϰ� �Ǹ� pv.IsMine�� �ٲ�鼭
            // �⺻���� a_SPos.m_SpawnTime = Random.Range(1.5f, 5.0f); <-- �̷��� �ʱ�ȭ �Ǿ� �����Ƿ�...
            // �ߺ� �����ϰ� �Ǵ� ������ �߻��Ѵ�.
            // �� �ٸ� ��ǻ�Ͱ� ������ Ŭ���̾�Ʈ�� �μ��ΰ� ���� ��,
            // m_SpawnTime �̰͵� �μ��ΰ� �޾ƾ� �ϴϱ�...
            for (int i = 0; i < m_SpawnPos.Count; i++)
            {
                m_SpawnPos[i].m_SpawnTime = (float)stream.ReceiveNext();
            }
        }
    }// public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)

    //������ Ŭ���̾�Ʈ ����� ȣ��Ǵ� �Լ�
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

        //���ο� ������ Ŭ���̾�Ʈ���� ��� ���� ���¸� ��� �μ� �ΰ� �� �־�� �Ѵ�.
        g_NetDelay = 1.0f;
    }//public override void OnMasterClientSwitched(Player newMasterClient)

}//public class MonSpawn_Mgr : MonoBehaviourPunCallbacks, IPunObservable
