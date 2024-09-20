using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCannon : MonoBehaviour
{
    //Cannon 프리팹을 연결할 변수
    public GameObject cannon = null;
    //포탄 발사 사운드 파일
    private AudioClip fireSfx = null;
    //AudioSource 컴포넌트를 할당할 변수
    private AudioSource sfx = null;

    //cannon 발사 지점
    public Transform firePos;

    //PhotonView 컴포넌트를 할당할 변수
    private PhotonView pv = null;

    TankDamage m_TankDamage = null;

    void Awake()
    {
        //cannon 프리팹을 Resources 폴더에서 불러와 변수에 할당
        cannon = (GameObject)Resources.Load("Cannon");
        //포탄 발사 사운드 파일을 Resources 폴더에서 불러와 변수에 할당
        fireSfx = Resources.Load<AudioClip>("CannonFire");
        //AutioSource 컴포넌트를 할당
        sfx = GetComponent<AudioSource>();  
        //PhotonView 컴포넌트를 pv 변수에 할당
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
        //PhotonView가 자신의 것이고, 마우스 왼쪽 버튼 클릭 시 발사 로직 수행
        if(pv.IsMine && Input.GetMouseButtonDown(0))
        {
            //마우스가 UI 위에 있으면...
            if (GameMgr.IsPointerOverUIObject() == true)
                return;

            if (m_TankDamage != null && m_TankDamage.currHp <= 0) //죽어 있으면 못 쏘게...
                return;

            if(GameMgr.GState != GameState.G_Play)return;

            //자신의 탱크일 경우는 로컬함수를 호출해 포탄을 발사
            Fire();
            //원격 네트워크 플레이어의 탱크에 RPC로 원격으로 Fire 함수를 호출
            pv.RPC("Fire", RpcTarget.Others, null);
        }
    }

    [PunRPC]
    void Fire()
    {
        //발사 사운드 발생
        sfx.PlayOneShot(fireSfx, 0.2f);
        GameObject a_Cannon = Instantiate(cannon, firePos.position, firePos.rotation);
        a_Cannon.GetComponent<Cannon>().AttackerId = pv.Owner.ActorNumber;
        //Owner : 소유자
        //아바타 탱크들 입장에서도 지금 이 탱크의 소유자가 누군지?
        //이 탱크의 IsMine의 저장공간에 접근할 수 있는 방법
    }
}
