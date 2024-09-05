using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon_Ctrl : MonoBehaviour
{
    public float m_RotSpeed = 1.0f; // 회전 속도
    public Transform sphereTransform; // 천구의 Transform
    public Transform cannonBarrel; // 포신의 Transform
    public float sphereRadius = 350.0f; // 천구의 반경
    public LayerMask terrainLayer; // Terrian 레이어 마스크

    private SphereCollider sphereCollider;
    private float targetRotationX;
    private float currentVelocity;

    private void Start()
    {
        // 천구의 SphereCollider를 가져옵니다.
        sphereCollider = sphereTransform.GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            // SphereCollider가 없으면 추가하고 설정합니다.
            sphereCollider = sphereTransform.gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = sphereRadius;
            sphereCollider.isTrigger = true;
        }

        // 초기 목표 회전 각도를 0도로 설정합니다.
        targetRotationX = 0f;
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 천구의 레이캐스트를 발사합니다.
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayer))
        {
            // 마우스 위치에 따라 목표 회전 각도를 설정합니다.
            Vector3 direction = hit.point - cannonBarrel.position;
            float angle = Mathf.Atan2(direction.y, direction.z) * Mathf.Rad2Deg;
            targetRotationX = Mathf.Clamp(-angle, -60f, 3f); // 각도를 반대로 설정
        }

        // 포신을 서서히 목표 각도로 회전시킵니다.
        float currentRotationX = cannonBarrel.localEulerAngles.x;
        if (currentRotationX > 180f) currentRotationX -= 360f; // -180도에서 180도 사이로 변환
        float newRotationX = Mathf.SmoothDamp(currentRotationX, targetRotationX, ref currentVelocity, 0.1f);
        cannonBarrel.localEulerAngles = new Vector3(newRotationX, cannonBarrel.localEulerAngles.y, cannonBarrel.localEulerAngles.z);
    }
}
