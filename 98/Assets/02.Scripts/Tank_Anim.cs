using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank_Anim : MonoBehaviour
{
    //# 텍스쳐의 회전속도
    public float m_ScrollSpeed = 1.0f;
    Renderer _Renderer;

    void Start()
    {
        _Renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        var offset = Time.time * m_ScrollSpeed * Input.GetAxisRaw("Vertical");
        _Renderer.material.SetTextureOffset("_MainTex", new Vector2(0,offset));
        _Renderer.material.SetTextureOffset("_BumpMap", new Vector2(0, offset));
    }

}
