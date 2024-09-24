using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    public float rotSpeed = 100.0f;

    private RaycastHit hit;

    //PhotonView ������Ʈ ����
    private PhotonView pv = null;
    //���� ��Ʈ��ũ ��ũ�� ���� ȸ�� ������ ������ ����
    private Quaternion currRot = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        rotSpeed = 10.0f;

        pv = GetComponent<PhotonView>();

        //�ʱ� ȸ���� ����
        currRot = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine)
        {
            if (GameMgr.isFocus == false) //�� ������ â�� ��Ŀ���� ���� ���� ������...
                return;

            //float angle = -Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * rotSpeed;
            //transform.Rotate(angle, 0, 0);

            //���� ī�޶󿡼� ���콺 Ŀ���� ��ġ�� ĳ���õǴ� Ray�� ����
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

            //���� ���� ����
            Vector3 a_Angle = transform.localEulerAngles;
            if (a_Angle.x < 180.0f)  //���� ������ ���������� ���
            {
                if (5.0f < a_Angle.x)
                    a_Angle.x = 5.0f;
            }
            else  //���� ������ �ø����� ���
            {
                if (a_Angle.x < 330.0f) //���� �� ���̸� ������ ������ �� �ȴ�.
                    a_Angle.x = 330.0f;
            }
            transform.localEulerAngles = a_Angle;
            //���� ���� ����
        }//if (pv.IsMine)
        else
        {
            //���� ȸ�� �������� ���Ź��� �ǽð� ȸ�� ������ �ε巴�� ȸ��
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
