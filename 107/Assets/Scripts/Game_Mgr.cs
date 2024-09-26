using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

enum JoyStickType
{
    Fixed = 0,
    Flexible = 1,
    FlexibleSwitch = 2
}

public class Game_Mgr : MonoBehaviour
{
    #region Singleton
    public static Game_Mgr Inst;
    private void Awake()
    {
        Inst = this;
    }
    #endregion


    #region JoyStick (Fixed)
    JoyStickType m_JoyStickType = JoyStickType.Fixed;
    [Header("JS")]
    public GameObject m_JSBObj = null;
    public Image m_JoyStick = null;
    float m_Rad = 0.0f;
    Vector3 m_OriginPos = Vector3.zero;
    Vector3 m_Axis = Vector3.zero;
    Vector3 m_JsCacVec = Vector3.zero;
    float m_JsCacDist = 0.0f;
    #endregion

    #region JoyStick (Flexible) 
    [Header("JS Flexible")]
    public GameObject m_JSPickPanel;
    Vector3 m_PosJBack;
    Vector3 m_DirStick;
    #endregion

    #region Player
    public Hero_Ctrl m_Player = null;
    #endregion

    #region Attack
    public Button m_AttackBtn = null;
    public Button m_JumpBtn = null;
    #endregion


    void Start()
    {

        #region JoyStick (Fixed)
        if (m_JSBObj !=null && m_JoyStick != null
            && m_JSBObj.activeSelf == true && m_JSPickPanel.activeSelf == false)
        {
            m_JoyStickType = JoyStickType.Fixed;

            Vector3[] v = new Vector3[4];

            m_JSBObj.GetComponent<RectTransform>().GetWorldCorners(v);

            m_Rad = Vector3.Distance(v[0], v[2]) * 0.5f;

            m_Rad = m_Rad / 3.0f;


            m_OriginPos = m_JoyStick.transform.position;

            //## ���
            EventTrigger trigger = m_JoyStick.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            //�巡��
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((data) =>
            {
                OnDragDelegate((PointerEventData)data);
            });
            trigger.triggers.Add(entry);

            //�巡�� ��
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.EndDrag;
            entry.callback.AddListener((data) =>
            {
                OnEndDragDelegate((PointerEventData)data);
            });
            trigger.triggers.Add(entry);
        }
        #endregion

        // ���� ��ư�� ���� �̺�Ʈ ������ �߰�
        if (m_AttackBtn != null)
        {
            m_AttackBtn.onClick.AddListener(() =>
            {
                if (m_Player != null)
                {
                    m_Player.Attack();
                }
            });
        }

        // ���� ��ư�� ���� �̺�Ʈ ������ �߰�
        if (m_JumpBtn != null)
        {
            m_JumpBtn.onClick.AddListener(() =>
            {
               
            });
        }

    }

    #region Joystick (Fixed) Delegates
    public void OnJSDown()
    {
        if (m_JSBObj != null)
            m_JSBObj.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        if (m_JoyStick != null)
            m_JoyStick.color = new Color32(255, 255, 255, 255);
    }

    public void OnJSUp()
    {
        if (m_JSBObj != null)
            m_JSBObj.GetComponent<Image>().color = new Color32(255, 255, 255, 140);

        if (m_JoyStick != null)
            m_JoyStick.color = new Color32(255, 255, 255, 120);

    }

    void OnDragDelegate(PointerEventData a_Data)
    {
        if (m_JoyStick == null) return;
        m_JsCacVec = Input.mousePosition - m_OriginPos;
        m_JsCacVec.z = 0.0f;
        m_JsCacDist = m_JsCacVec.magnitude;
        m_Axis = m_JsCacVec.normalized;

        //���̽�ƽ ��׶��带 ������ʰ�
        if (m_JsCacDist > m_Rad)
        {
            m_JoyStick.transform.position = m_OriginPos + m_Axis * m_Rad;
        }
        else
        {
            m_JoyStick.transform.position = m_OriginPos + m_Axis * m_JsCacDist;
        }

        if (1 < m_JsCacDist)
            m_JsCacDist = 1.0f;


        //�̵�
        if (m_Player != null)
            m_Player.SetJsMv(m_JsCacDist, m_Axis);
    }

    void OnEndDragDelegate(PointerEventData a_Data)
    {
        if (m_JoyStick == null) return;

        m_Axis = Vector3.zero;
        m_JoyStick.transform.position = m_OriginPos;
        m_JsCacDist = 0.0f;

        // �÷��̾��� ���̽�ƽ �̵� ���� �ʱ�ȭ
        if (m_Player != null)
            m_Player.SetJsMv(0.0f, Vector3.zero);
    }

    #endregion


    #region JoyStick (Flexible)
    #endregion

    #region ���콺 Ŭ�� ����
    public static bool IsPointerOverUIObject() //UGUI�� UI���� ���� ��ŷ�Ǵ��� Ȯ���ϴ� �Լ�
    {
        PointerEventData a_EDCurPos = new PointerEventData(EventSystem.current);

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)

			List<RaycastResult> results = new List<RaycastResult>();
			for (int i = 0; i < Input.touchCount; ++i)
			{
				a_EDCurPos.position = Input.GetTouch(i).position;  
				results.Clear();
				EventSystem.current.RaycastAll(a_EDCurPos, results);
                if (0 < results.Count)
                    return true;
			}

			return false;
#else
        a_EDCurPos.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(a_EDCurPos, results);
        return (0 < results.Count);
#endif
    }//public bool IsPointerOverUIObject()
    #endregion
}



