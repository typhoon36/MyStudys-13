using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPos   //List로 관리하고 List이기 때문에 인덱스를 갖게 된다.
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

// 지형에 설치형들은 마스터 클라이언트가 제어한다.
// 그래서 InstantiateRoomObject 로 만들어진게 아니라면
// 마스터 클라이언트가 나갔다가 들어오면 정상적으로 인수인계 되게 하고 
// 싶기 때문에 이 클래스도 InstantiateRoomObject 로 만들어 주는 게 좋다고 판단함

public class MonSpawn_Mgr : MonoBehaviourPunCallbacks, IPunObservable
{
    PhotonView pv = null;
    List<SpawnPos> m_SpawnPos = new List<SpawnPos>();

    float g_NetDelay = 0.0f;

    //--- 싱글턴 패턴
    public static MonSpawn_Mgr Inst = null;

    void Awake()
    {
        Inst = this; 

        pv = GetComponent<PhotonView>();
    }
    //--- 싱글턴 패턴

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
                a_SPos.m_SpawnTime = Random.Range(1.5f, 5.0f);  //1.5초에서 5.0초 뒤에 생성되게 해 준다.
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
            return;     //동기화 가능한 상태 일때만 업데이트를 계산해 준다.

        if (PhotonNetwork.IsMasterClient == false)
            return;     //마스터 클라이언트가 아니면 스폰을 관리 하지 못하게 한다.

        if (0.0f < g_NetDelay)
            g_NetDelay -= Time.deltaTime;

        if (0.0f < g_NetDelay)
            return;    //마스터 변경시 스폰 잠시 딜레이 주기 마스터 정보 동기화 위해...

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
        if(stream.IsWriting)  //pv.IsMine 즉 PhotonNetwork.IsMasterClient 이다.
        {
            for(int i = 0; i < m_SpawnPos.Count; i++)
            {
                stream.SendNext(m_SpawnPos[i].m_SpawnTime);
            }
        }
        else
        {
            // PhotonNetwork.IsMasterClient 가 방을 나가면서...
            // pv.IsMine 즉 PhotonNetwork.IsMasterClient 가 바뀌어도 m_SpawnTime도 
            // 인수 인계 받기 위하여... 미리 다른 컴퓨터들도 계속 받아 놓는다.
            // 이 부분을 안하게 되면 pv.IsMine이 바뀌면서
            // 기본값이 a_SPos.m_SpawnTime = Random.Range(1.5f, 5.0f); <-- 이렇게 초기화 되어 있으므로...
            // 중복 스폰하게 되는 현상이 발생한다.
            // 즉 다른 컴퓨터가 마스터 클라이언트를 인수인계 받을 때,
            // m_SpawnTime 이것도 인수인계 받아야 하니까...
            for (int i = 0; i < m_SpawnPos.Count; i++)
            {
                m_SpawnPos[i].m_SpawnTime = (float)stream.ReceiveNext();
            }
        }
    }// public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)

    //마스터 클라이언트 변경시 호출되는 함수
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

        //새로운 마스터 클라이언트에게 모든 변수 상태를 즉시 인수 인계 해 주어야 한다.
        g_NetDelay = 1.0f;
    }//public override void OnMasterClientSwitched(Player newMasterClient)

}//public class MonSpawn_Mgr : MonoBehaviourPunCallbacks, IPunObservable
