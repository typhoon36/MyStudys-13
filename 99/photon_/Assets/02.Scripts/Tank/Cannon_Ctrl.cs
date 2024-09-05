using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon_Ctrl : MonoBehaviour
{
    public float m_RotSpeed = 1.0f; // ȸ�� �ӵ�
    public Transform sphereTransform; // õ���� Transform
    public Transform cannonBarrel; // ������ Transform
    public float sphereRadius = 350.0f; // õ���� �ݰ�
    public LayerMask terrainLayer; // Terrian ���̾� ����ũ

    private SphereCollider sphereCollider;
    private float targetRotationX;
    private float currentVelocity;

    private void Start()
    {
        // õ���� SphereCollider�� �����ɴϴ�.
        sphereCollider = sphereTransform.GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            // SphereCollider�� ������ �߰��ϰ� �����մϴ�.
            sphereCollider = sphereTransform.gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = sphereRadius;
            sphereCollider.isTrigger = true;
        }

        // �ʱ� ��ǥ ȸ�� ������ 0���� �����մϴ�.
        targetRotationX = 0f;
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // õ���� ����ĳ��Ʈ�� �߻��մϴ�.
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayer))
        {
            // ���콺 ��ġ�� ���� ��ǥ ȸ�� ������ �����մϴ�.
            Vector3 direction = hit.point - cannonBarrel.position;
            float angle = Mathf.Atan2(direction.y, direction.z) * Mathf.Rad2Deg;
            targetRotationX = Mathf.Clamp(-angle, -60f, 3f); // ������ �ݴ�� ����
        }

        // ������ ������ ��ǥ ������ ȸ����ŵ�ϴ�.
        float currentRotationX = cannonBarrel.localEulerAngles.x;
        if (currentRotationX > 180f) currentRotationX -= 360f; // -180������ 180�� ���̷� ��ȯ
        float newRotationX = Mathf.SmoothDamp(currentRotationX, targetRotationX, ref currentVelocity, 0.1f);
        cannonBarrel.localEulerAngles = new Vector3(newRotationX, cannonBarrel.localEulerAngles.y, cannonBarrel.localEulerAngles.z);
    }
}
