using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityStandardAssets.Utility;

public class TankMove : MonoBehaviourPunCallbacks, IPunObservable
{
    //탱크의 이동 및 회전 속도를 나타내는 변수
    public float moveSpeed = 20.0f;
    public float rotSpeed = 50.0f;

    //참조할 컴포넌트를 할당할 변수
    private Rigidbody rbody;
    //키보드 입력값 변수
    private float h, v;

    //PhotonView 컴포넌트를 할당할 변수
    private PhotonView pv = null;
    //메인 카메라가 추적할 CamPivot 게임오브젝트
    public Transform camPivot;

    //위치 정보를 송수신할 때 사용할 변수 선언 및 초깃값 설정
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    //--- 탱크의 충돌 예외 처리를 위한 변수
    TankDamage m_TankDamage = null;
    Terrain m_RefMap = null;
    Vector3 m_CacPos = Vector3.zero;
    //--- 탱크의 충돌 예외 처리를 위한 변수

    // Start is called before the first frame update
    void Start()
    {
        //컴포넌트 할당
        rbody = GetComponent<Rigidbody>();

        //Rigidbody의 무게중심을 낮게 설정
        rbody.centerOfMass = new Vector3(0.0f, -2.5f, 0.0f);

        //PhotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();

        //PhotonView가 자신의 탱크일 경우
        if(pv.IsMine)
        {
            //메인 카메라에 추가된 SmoothFollow 스크립트에 추적 대상을 연결
            Camera.main.GetComponent<SmoothFollow>().target = camPivot;
        }
        else
        {
            //원격 네트워크 플레이어의 탱크(아바타)는 물리력을 이용하지 않음
            rbody.isKinematic = true;
        }

        //원격 탱크의 위치 및 회전 값을 처리할 변수의 초기값 설정
        currPos = transform.position;
        currRot = transform.rotation;

        //--- 탱크의 충돌 예외 처리를 위한 변수
        m_TankDamage = GetComponent<TankDamage>();
        m_RefMap = FindObjectOfType<Terrain>();
        //--- 탱크의 충돌 예외 처리를 위한 변수

    }//void Start()

    // Update is called once per frame
    void Update()
    {
        //자신이 만든 네트워크 게임오브젝트가 아닌 경우는 키보드 조작 루틴을 나감
        if (pv.IsMine)
        {
            if (GameMgr.bEnter == true)
                return;

            if(GameMgr.GState != GameState.G_Play)
                return;

            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            //회전과 이동 처리
            transform.Rotate(Vector3.up * rotSpeed * h * Time.deltaTime);
            transform.Translate(Vector3.forward * v * moveSpeed * Time.deltaTime);
            //rbody.angularVelocity = Vector3.up * rotSpeed * h;
            //Vector3 a_Move = Vector3.forward * v * moveSpeed;
            //rbody.velocity = new Vector3(a_Move.x, rbody.velocity.y, a_Move.z);

            UnitCollUpdate();
        }
        else  //원격 플레이어(아바타)일 때 수행
        {
            if (10.0f < (transform.position - currPos).magnitude)
            {
                //중계 받은 좌표와 현재좌표의 거리 차가 10m 이상이면 즉시 저프 보정
                transform.position = currPos;
            }
            else
            {
                //원격 플레이어의 탱크(아바타)를 수신받은 위치까지 부드럽게 이동시킴
                transform.position = Vector3.Lerp(transform.position, currPos, Time.deltaTime * 10.0f);
            }

            //원격 플레이어의 탱크(아바타)를 수신받은 각도만큼 부드럽게 회전시킴
            transform.rotation = Quaternion.Slerp(transform.rotation, currRot, Time.deltaTime * 10.0f);
        }//else  //원격 플레이어(아바타)일 때 수행

    } //void Update()

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //로컬 플레이어(IsMine)의 위치 정보 송신
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else //원격 플레이어(아바타)의 위치 정보 수신
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

        //--- 지형 밑으로 빠졌을 때...
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
        //--- 지형 밑으로 빠졌을 때...

        //--- 탱크가 지형 밖으로 벗어나지 못하게 막기...
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
        //--- 탱크가 지형 밖으로 벗어나지 못하게 막기...

        //--- 탱크가 뒤집어 지려고 하면 바로 세워준다.
        if(transform.position.y < (a_CurHeight + 8.0f))
        if(Vector3.Dot(transform.up, Vector3.up) <= 0.2f)
        {
            transform.up = Vector3.up;
            rbody.angularVelocity = new Vector3(0, 0, 0);
        }
        //--- 탱크가 뒤집어 지려고 하면 바로 세워준다.

    }//void UnitCollUpdate()
}
