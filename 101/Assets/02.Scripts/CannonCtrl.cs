using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    public float rotSpeed = 100.0f;

    private RaycastHit hit;

    //PhotonView 컴포넌트 변수
    private PhotonView pv = null;
    //원격 네트워크 탱크의 포신 회전 각도를 저장할 변수
    private Quaternion currRot = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        rotSpeed = 10.0f;

        pv = GetComponent<PhotonView>();

        //초기 회전값 설정
        currRot = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine)
        {
            if (GameMgr.isFocus == false) //이 윈도우 창이 포커스를 갖고 있지 않으면...
                return;

            //float angle = -Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * rotSpeed;
            //transform.Rotate(angle, 0, 0);

            //메인 카메라에서 마우스 커서의 위치로 캐스팅되는 Ray를 생성
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity,
                                                1 << LayerMask.NameToLayer("TERRAIN")) == true)
            {
                Vector3 a_CacVec = hit.point - transform.position;
                Quaternion a_Rotate = Quaternion.LookRotation(a_CacVec.normalized);
                a_Rotate.eulerAngles = new Vector3(a_Rotate.eulerAngles.x,
                                        transform.eulerAngles.y, transform.eulerAngles.z);
                transform.rotation = Quaternion.Slerp(transform.rotation, a_Rotate, Time.deltaTime * rotSpeed);
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0.0f, 0.0f);
            }
            else
            {
                Vector3 a_QrgVec = ray.origin + ray.direction * 2000.0f;
                ray = new Ray(a_QrgVec, -ray.direction);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity,
                                                1 << LayerMask.NameToLayer("TURRETPICKOBJ")) == true)
                {
                    Vector3 a_CacVec = hit.point - transform.position;
                    Quaternion a_Rotate = Quaternion.LookRotation(a_CacVec.normalized);
                    a_Rotate.eulerAngles = new Vector3(a_Rotate.eulerAngles.x,
                                            transform.eulerAngles.y, transform.eulerAngles.z);
                    transform.rotation = Quaternion.Slerp(transform.rotation, a_Rotate, Time.deltaTime * rotSpeed);

                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0.0f, 0.0f);
                }
            }//else

            //포신 각도 제한
            Vector3 a_Angle = transform.localEulerAngles;
            if (a_Angle.x < 180.0f)  //포신 각도가 내려가려는 경우
            {
                if (5.0f < a_Angle.x)
                    a_Angle.x = 5.0f;
            }
            else  //포신 각도를 올리려는 경우
            {
                if (a_Angle.x < 330.0f) //값을 더 줄이면 각도가 제한이 더 된다.
                    a_Angle.x = 330.0f;
            }
            transform.localEulerAngles = a_Angle;
            //포신 각도 제한
        }//if (pv.IsMine)
        else
        {
            //현재 회전 각도에서 수신받은 실시간 회전 각도로 부드럽게 회전
            transform.localRotation = Quaternion.Slerp(transform.localRotation,
                                                    currRot, Time.deltaTime * 10.0f);
        }

    }//void Update()

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(transform.localRotation);
        }
        else
        {
            currRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
