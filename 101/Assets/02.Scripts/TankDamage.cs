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
    //탱크 폭파 후 투명 처리를 위한 MeshRenderer 컴포넌트 배열
    private MeshRenderer[] renderers;

    //탱크 폭발 효과 프리팹을 연결할 변수
    private GameObject expEffect = null;

    //탱크의 초기 생명치
    private int initHp = 200;
    //탱크의 현재 생명치
    [HideInInspector] public int currHp = 200;
    int NetHp = 200;    //아바타 탱크(원격지 탱크)들의 Hp 값을 동기화 시켜주기 위한 변수
    //아바타 탱크 입장에서는 네트웍으로 Hp 값을 받아서 죽는 시점을 파악해서 사망처리 하기 위해...

    //탱크 하위의 Canvas 객체를 연결할 변수
    public Canvas hudCanvas;
    //Filled 타입의 ImageUI 항목을 연결할 변수
    public Image hpBar;

    PhotonView pv = null;

    //플레이어 ID(고유번호) 저장하는 변수
    [HideInInspector] public int PlayerId = -1;

    //적 탱크 파괴 스코어를 CustomProperties를 통해 중계하기 위한 변수들...
    int m_KillCount = 0;    //모든 컴퓨터의 내 탱크들의 변수
    int m_Cur_LAttId = -1;  //누가 마지막 공격(막타) 했는지? Last Attack Id

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
        PlayerId = pv.Owner.ActorNumber;    //플레이어의 고유번호를 하나 저장해 둠

        //탱크 모델의 모든 MeshRenderer 컴포넌트를 추출한 후 배열에 할당
        renderers = GetComponentsInChildren<MeshRenderer>();

        //현재 생명치를 초기 생명치로 초기값 설정
        currHp = initHp;

        //탱크 폭발 시 생성시킬 폭발 효과를 로드
        expEffect = Resources.Load<GameObject>("ExplosionMobile");

        //Filled 이미지 색상을 녹색으로 설정
        hpBar.color = Color.green;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.CurrentRoom == null ||
            PhotonNetwork.LocalPlayer == null)
            return;
        //동기화 가능한 상태 일때만 업데이트를 계산해 준다.

        //--- 원격 플레이어(아바타 탱크 입장) 일 때 동기화 코드
        if (pv.IsMine == false)
        { //원격 플레이어(아바타 탱크 입장) 일 때 수행
            AvataUpdate();
        }
        //--- 원격 플레이어(아바타 탱크 입장) 일때 동기화 코드
    }

    void OnTriggerEnter(Collider coll)
    {
        //충돌한 Collider의 태크 비교
        if(currHp > 0 && coll.tag == "CANNON")
        {
            int a_Att_Id = -1;
            Cannon a_RefCannon = coll.gameObject.GetComponent<Cannon>();
            if (a_RefCannon != null)
                a_Att_Id = a_RefCannon.AttackerId;

            TakeDamage(a_Att_Id);

            //currHp -= 20;

            ////현재 생명치 백분율 = (현재 생명치) / (초기 생명치)
            //hpBar.fillAmount = (float)currHp / (float)initHp;

            ////생명 수치에 따라 Filled 이미지의 색상을 변경
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
        //자기가 쏜 총알은 자신이 맞으면 안되도록 처리...
        if (AttackerId == PlayerId)
            return;

        if (currHp <= 0.0f)
            return;

        //피격연출

        if (pv.IsMine == false) //원격지 탱크(아바타)면 제외
            return;

        //pv.IsMine 일 때
        m_Cur_LAttId = AttackerId;
        currHp -= 20;
        if (currHp < 0)
            currHp = 0;

        //현재 생명치 백분율 = (현재 생명치) / (초기 생명치)
        hpBar.fillAmount = (float)currHp / (float)initHp;

        //생명 수치에 따라 Filled 이미지의 색상을 변경
        if (hpBar.fillAmount <= 0.4f)
            hpBar.color = Color.red;
        else if (hpBar.fillAmount <= 0.6f)
            hpBar.color = Color.yellow;
        else
            hpBar.color = Color.green;

        if (currHp <= 0)  //죽는 처리 (아바타 탱크들은 중계 받아서 처리)
        {
            //IsMine 기준에서 죽는 처리
            StartCoroutine(this.ExplosionTank());
        }
    }

    //폭발 효과 생성 및 리스폰 코루틴 함수
    IEnumerator ExplosionTank()
    {
        //폭발 효과 생성
        GameObject effect = GameObject.Instantiate(expEffect,
                                            transform.position, 
                                            Quaternion.identity);
        Destroy(effect, 3.0f);

        //HUD를 비활성화
        hudCanvas.enabled = false;

        //탱크 투명 처리
        SetTankVisible(false);

        if (pv != null && pv.IsMine == true)  //되살리는 건 IsMine 일때만 하고
        {
            //10초 동안 기다렸다가 활성화하는 로직을 수행
            yield return new WaitForSeconds(10.0f);

            //Filled 이미지 초깃값으로 환원
            hpBar.fillAmount = 1.0f;
            //Filled 이미지 색상을 녹색으로 설정
            hpBar.color = Color.green;
            //HUD 활성화
            hudCanvas.enabled = true;

            //리스폰 시 생명 초깃값 설정
            currHp = initHp;
            //탱크를 다시 보이게 처리
            SetTankVisible(true);
        }
        else
        {
            //아바타 탱크들일 때는 중계 받아서 되살리겠다는 뜻
            yield return null;  //한 플레임 정도만 돌고 코루틴 함수 다음을 실행한다.
        }
    }

    //MeshRenderer를 활성화/비활성화하는 함수
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
        if(stream.IsWriting)  //IsMine쪽 정보 송신
        {
            stream.SendNext(m_Cur_LAttId); //이번에 -20 깍인 게 누구 때문인지 같이 보내준다는 의도
            stream.SendNext(currHp);
        }
        else //원격 플레이어(아바타)의 정보 수신
        {
            m_Cur_LAttId = (int)stream.ReceiveNext(); //아바타들도 누구 때문에 hp가 깍인 건지 알게 한다.
            NetHp = (int)stream.ReceiveNext();
            //아바타 입장에서 사망 시점을 알기 위해 NetHp 라는 변수를 따로 만들어서
            //IsMine에서 송신해 준 Hp 값을 받았다.
        }
    }// public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)

    void AvataUpdate()  //원격지 플레이어 Hp Update 처리 함수
    {
        if(0 < currHp)
        {
            currHp = NetHp;

            //현재 생명치 백분율 = (현재 생명치) / (초기 생명치)
            hpBar.fillAmount = (float)currHp / (float)initHp;

            //생명 수치에 따라 Filled 이미지의 색상을 변경
            if (hpBar.fillAmount <= 0.4f)
                hpBar.color = Color.red;
            else if (hpBar.fillAmount <= 0.6f)
                hpBar.color = Color.yellow;
            else
                hpBar.color = Color.green;

            if(currHp <= 0) //죽는 처리 (아바타 탱크들은 중계 받아서 처리)
            {
                currHp = 0;

                if(0 <= m_Cur_LAttId)  //공격자 Id가 유효할 때
                { //지금 Hp가 깎이게 해서 사망에 이르게 한 탱크가 누구인지?
                    // 지금 죽은 탱크 입장에서는 
                    // 아바타들 중에서 AttackerId (<--- IsMine)을 찾아서
                    // KillCount를 증가시켜 줘야 한다.
                    // 자신을 파괴시킨 적 탱크의 스코어를 증가시키는 함수를 호출

                }

                //IsMine 기준에서 죽는 처리
                StartCoroutine(this.ExplosionTank());
            }

        } //if(0 < currHp)
        else //if(currHp <= 0) 죽어 있는 상황에서
        { //죽어 있을 때 계속 NetHp는 0으로 계속 들어오게 되고
            //되살려야 하는 상황 처리
            currHp = NetHp;
            if((int)(initHp * 0.95f) < currHp) //이번에 들어온 Hp가 최대 에너지가 들어오면
            {   //되살려야 하는 상황으로 판단하겠다는 뜻

                //Filled 이미지 초기값으로 환원
                hpBar.fillAmount = 1.0f;
                //Filled 이미지 색상을 녹색으로 설정
                hpBar.color = Color.green;
                //HUD 활성화
                hudCanvas.enabled = true;

                //리스폰 시 새 생명 초기값 설정
                currHp = initHp;
                //탱크를 다시 보이게 처리
                SetTankVisible(true);

            }//if ((int)(initHp * 0.95f) < currHp) //이번에 들어온 Hp가 최대 에너지가 들어오면
        }//else //if(currHp <= 0) 죽어 있는 상황에서

    }//void AvataUpdate()  //원격지 플레이어 Hp Update 처리 함수
}
