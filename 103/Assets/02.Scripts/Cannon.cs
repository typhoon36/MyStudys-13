using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    //��ź�� �ӵ�
    public float speed = 6000.0f;
    //���� ȿ�� ������ ���� ����
    public GameObject expEffect;
    private CapsuleCollider _collider;
    private Rigidbody _rigidbody;

    //��ź�� �߻��� �÷��̾��� ID ����
    [HideInInspector] public int AttackerId = -1;
    //���� �� �Ѿ������� �����ϱ� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<CapsuleCollider>();
        _rigidbody = GetComponent<Rigidbody>(); 

        GetComponent<Rigidbody>().AddForce(transform.forward * speed);

        //3�ʰ� ���� �� �ڵ� �����ϴ� �ڷ�ƾ ����
        StartCoroutine(this.ExplosionCannon(3.0f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "CANNON")
            return;

        //���� �Ǵ� �� ��ũ�� �浹�� ��� ��� �����ϵ��� �ڷ�ƾ ����
        StartCoroutine(this.ExplosionCannon(0.0f));
    }

    IEnumerator ExplosionCannon(float tm)
    {
        yield return new WaitForSeconds(tm);
        //�浹 �ݺ� �Լ��� �߻����� �ʵ��� Collider�� ��Ȱ��ȭ
        if(_collider != null) 
            _collider.enabled = false;

        //���������� ������ ���� �ʿ� ����
        if (_rigidbody != null)
        {
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = new Vector3(0, 0, 0);
            _rigidbody.isKinematic = true;
        }

        Vector3 a_CacDist = transform.position - Camera.main.transform.position;
        float a_BackLen = a_CacDist.magnitude * 0.1f;
        if (9.0f < a_BackLen)
            a_BackLen = 9.0f;

        //���� ������ ���� ����
        GameObject obj = (GameObject)Instantiate(expEffect,
                                transform.position - (transform.forward * a_BackLen),
                                Quaternion.identity);

        Destroy(obj, 1.0f);

        //Trail Renderer�� �Ҹ�Ǳ���� ��� ��� �� ���� ó��
        Destroy(this.gameObject, 1.0f);
    }
}
