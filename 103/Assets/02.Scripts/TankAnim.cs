using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAnim : MonoBehaviour
{
    //�ؽ�ó�� ȸ���ӵ�
    private float scrollSpeed = 1.0f;
    private Renderer _renderer;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        var offset = Time.time * scrollSpeed * Input.GetAxisRaw("Vertical");

        //�⺻ �ؽ�ó�� Y ������ �� ����
        _renderer.material.SetTextureOffset("_MainTex", new Vector2(0, offset));
        //�븻 �ؽ�ó�� Y ������ �� ����
        _renderer.material.SetTextureOffset("_BumpMap", new Vector2(0, offset));
    }
}
