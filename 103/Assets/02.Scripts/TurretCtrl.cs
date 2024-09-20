using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    //����(Ray)�� ���鿡 ���� ��ġ�� ������ ����
    private RaycastHit hit;

    //�ͷ��� ȸ�� �ӵ�
    public float rotSpeed = 5.0f;

    //PhotonView ������Ʈ ����
    private PhotonView pv = null;
    //���� ��Ʈ��ũ ��ũ�� �ͷ� ȸ������ ������ ����
    private Quaternion currRot = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();

        //�ʱ� ȸ���� ����
        currRot = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine) //�ڱⰡ �����ϰ� �ִ� ��ũ�� ���
        {
            if (GameMgr.isFocus == false) //�� ������ â�� ��Ŀ���� ���� ���� ������...
                return;

            //���� ī�޶󿡼� ���콺 Ŀ���� ��ġ�� ĳ���õǴ� Ray�� ����
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //������ Ray�� Scene �信 ��� �������� ǥ��
            Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.green);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("TERRAIN")) == true)
            {
                //Ray�� ���� ��ġ�� ������ǥ�� ��ȯ
                Vector3 relative = transform.InverseTransformPoint(hit.point);
                //��ź��Ʈ �Լ��� Atan2�� �� �� ���� ������ ���
                float angle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
                //rotSpeed ������ ������ �ӵ��� ȸ��
                transform.Rotate(0, angle * Time.deltaTime * rotSpeed, 0);
            }
            else
            {
                Vector3 a_OrgVec = ray.origin + ray.direction * 2000.0f;
                ray = new Ray(a_OrgVec, -ray.direction);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity,
                                    1 << LayerMask.NameToLayer("TURRETPICKOBJ")) == true)
                {
                    //Ray�� ���� ��ġ�� ������ǥ�� ��ȯ
                    Vector3 relative = transform.InverseTransformPoint(hit.point);
                    //��ź��Ʈ �Լ��� Atan2�� �� �� ���� ������ ���
                    float angle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
                    //rotSpeed ������ ������ �ӵ��� ȸ��
                    transform.Rotate(0, angle * Time.deltaTime * rotSpeed, 0);
                }
            }//else
        }//if (pv.IsMine)
        else  //���� ��Ʈ��ũ �÷��̾�(�ƹ�Ÿ) ��ũ�� ���
        {
            //���� ȸ���������� ���Ź��� �ǽð� ȸ�������� �ε巴�� ȸ��
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
