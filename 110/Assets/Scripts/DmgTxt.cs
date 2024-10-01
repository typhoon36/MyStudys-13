using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DmgTxt : MonoBehaviour
{
    [HideInInspector] public Text m_RTxt;
    [HideInInspector] public float m_DmgVal = 0;
    [HideInInspector] public Vector3 m_BaseWdPos = Vector3.zero;

    Animator m_Anim;

    RectTransform m_CanvasRect;

    Vector2 m_ScreenPos = Vector2.zero;
    Vector2 m_WdScPos = Vector2.zero;

    void Start()
    {

        m_RTxt = this.gameObject.GetComponentInChildren<Text>();
        if (m_RTxt != null)
        {
            m_RTxt.text = "-" + m_DmgVal.ToString();
        }

        m_Anim = GetComponentInChildren<Animator>();

        if (m_Anim != null)
        {
            AnimatorStateInfo a_AnimStateInfo =
                m_Anim.GetCurrentAnimatorStateInfo(0);

            float a_LifeTime = a_AnimStateInfo.length;

            Destroy(this.gameObject, a_LifeTime);
        }
    }

    Vector3 m_CacVec = Vector3.zero;

    void LateUpdate()
    {
        // 월드 좌표를 스크린 좌표로 변환
        m_CanvasRect = GameMgr.Inst.m_Dmg_Canvas.GetComponent<RectTransform>();
        m_ScreenPos = Camera.main.WorldToScreenPoint(m_BaseWdPos);

        // 스크린 좌표를 캔버스 좌표로 변환
        m_WdScPos.x = ((m_ScreenPos.x / Screen.width) * m_CanvasRect.sizeDelta.x) - (m_CanvasRect.sizeDelta.x * 0.5f);
        m_WdScPos.y = ((m_ScreenPos.y / Screen.height) * m_CanvasRect.sizeDelta.y) - (m_CanvasRect.sizeDelta.y * 0.5f);

        transform.GetComponent<RectTransform>().anchoredPosition = m_WdScPos;

        // 카메라 컬링
        m_CacVec = m_BaseWdPos - Camera.main.transform.position;

        if (m_CacVec.magnitude <= 0)
        {
            if (m_RTxt.gameObject.activeSelf)
                m_RTxt.gameObject.SetActive(false);
        }
        else if (0 < Vector3.Dot(Camera.main.transform.forward, m_CacVec.normalized))
        {
            if (m_RTxt.gameObject.activeSelf == false)
                m_RTxt.gameObject.SetActive(true);
        }
        else
        {
            if (m_RTxt.gameObject.activeSelf)
                m_RTxt.gameObject.SetActive(false);
        }
    }

}
