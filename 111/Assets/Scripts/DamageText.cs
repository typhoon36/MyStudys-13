using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
    [HideInInspector] public Text m_RefText = null;
    [HideInInspector] public float m_DamageVal = 0.0f;
    [HideInInspector] public Vector3 m_BaseWdPos = Vector3.zero;

    Animator m_RefAnimator = null;

    RectTransform CanvasRect;
    Vector2 ScreenPos = Vector2.zero;   
    Vector2 WdScPos = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        m_RefText = this.gameObject.GetComponentInChildren<Text>();
        if(m_RefText != null)
        {
            m_RefText.text = "-" + m_DamageVal.ToString() + " Dmg";
        }

        m_RefAnimator = GetComponentInChildren<Animator>();
        if(m_RefAnimator != null)
        {
            AnimatorStateInfo a_AnimStateInfo =
                            m_RefAnimator.GetCurrentAnimatorStateInfo(0);
            float a_LifeTime = a_AnimStateInfo.length;
            Destroy(gameObject, a_LifeTime);
        }
    }//void Start()

    //// Update is called once per frame
    //void Update()
    //{

    //}

    Vector3 m_CacVec = Vector3.zero;
    void LateUpdate()
    {
        //---World ��ǥ�� UGUI ��ǥ�� ȯ���� �ִ� �ڵ�
        CanvasRect = GameMgr.Inst.m_Damage_Canvas.GetComponent<RectTransform>();
        ScreenPos = Camera.main.WorldToViewportPoint(m_BaseWdPos);
        WdScPos.x = ((ScreenPos.x * CanvasRect.sizeDelta.x) -
                                        (CanvasRect.sizeDelta.x * 0.5f));
        WdScPos.y = ((ScreenPos.y * CanvasRect.sizeDelta.y) -
                                        (CanvasRect.sizeDelta.y * 0.5f));
        transform.GetComponent<RectTransform>().anchoredPosition = WdScPos;
        //---World ��ǥ�� UGUI ��ǥ�� ȯ���� �ִ� �ڵ�

        //--- ī�޶� �ø�
        m_CacVec = m_BaseWdPos - Camera.main.transform.position;
        if(m_CacVec.magnitude <= 0.0f)
        {   //�������ؽ�Ʈ�� ī�޶� ���� ��ġ�� �־ ���� �ʿ� ����

            if(m_RefText.gameObject.activeSelf == true)
                m_RefText.gameObject.SetActive(false);
        }
        else if(0.0f < Vector3.Dot(Camera.main.transform.forward, m_CacVec.normalized))
        {   //ī�޶� ���ʿ� �ִٴ� ��
            if (m_RefText.gameObject.activeSelf == false)
                m_RefText.gameObject.SetActive(true);
        }
        else //if(Vector3.Dot(Camera.main.transform.forward, m_CacVec.normalized) <= 0.0f)
        {   //ī�޶� ���ʿ� �ִٴ� ��
            if (m_RefText.gameObject.activeSelf == true)
                m_RefText.gameObject.SetActive(false);
        }
        //--- ī�޶� �ø�

    }//void LateUpdate()

}
