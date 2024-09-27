using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

enum JoyStickType
{
    Fixed = 0,
    Flexible = 1,
    FlexibleOnOff = 2
}

public class GameMgr : MonoBehaviour
{


    [HideInInspector] public Hero_Ctrl m_RefHero = null;

    #region Cursor Mark
    public GameObject m_CursorMark = null;
    Vector3 m_CacLenV = Vector3.zero;
    #endregion

    #region Fixed JoyStick 처리 부분
    JoyStickType m_JoyStickType = JoyStickType.Fixed;
    [Header("--- JoyStick ---")]
    public GameObject m_JoySBackObj = null;
    public Image m_JStickImg = null;
    float m_Radius = 0.0f;
    Vector3 m_OriginPos = Vector3.zero;
    Vector3 m_Axis = Vector3.zero;
    Vector3 m_JsCacVec = Vector3.zero;
    float m_JsCacDist = 0.0f;
    #endregion

    #region Flexible JoyStick 처리 부분
    public GameObject m_JoystickPickPanel = null;
    Vector3 posJoyBack;
    Vector3 dirStick;
    #endregion

    #region Attack&Skill Button
    [Header("#Buttons")]
    public Button m_BackBtn;
    public Button m_AttBtn;
    public Button m_SkillBtn;
    #endregion

    #region Singleton
    public static GameMgr Inst = null;

    void Awake()
    {
        Inst = this;
    }
    #endregion

  
    void Start()
    {
        #region Button
        if (m_BackBtn != null)
        {
            m_BackBtn.onClick.AddListener(() =>
            {

            });
        }

        if (m_AttBtn != null)
        {
            m_AttBtn.onClick.AddListener(() =>
            {
                if (m_RefHero != null)
                    m_RefHero.Attack();
            });
        }

        if (m_SkillBtn != null)
        {
            m_SkillBtn.onClick.AddListener(() =>
            {

            });
        }
        #endregion


        #region Fixed JoyStick 처리 부분
        if (m_JoySBackObj != null && m_JStickImg != null &&
            m_JoySBackObj.activeSelf == true &&
            m_JoystickPickPanel.activeSelf == false)
        {
            m_JoyStickType = JoyStickType.Fixed;

            Vector3[] v = new Vector3[4];
            m_JoySBackObj.GetComponent<RectTransform>().GetWorldCorners(v);
            //[0] : 좌측하단  [1] : 좌측상단  [2] : 우측상단   [3] : 우측하단
            m_Radius = v[2].y - v[0].y;
            m_Radius = m_Radius / 3.0f;

            m_OriginPos = m_JStickImg.transform.position;

            //스크립트로만 대기하고자 할 때
            EventTrigger trigger = m_JoySBackObj.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = m_JoySBackObj.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((data) =>
            {
                OnDragJoyStick((PointerEventData)data);
            });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.EndDrag;
            entry.callback.AddListener((data) =>
            {
                OnEndDragJoyStick((PointerEventData)data);
            });
            trigger.triggers.Add(entry);
        }
        #endregion

        #region Flexible Joystick
        if (m_JoystickPickPanel != null && m_JoySBackObj != null
            && m_JStickImg != null &&
            m_JoystickPickPanel.activeSelf == false)
        {
            if (m_JoySBackObj.activeSelf == true)
                m_JoyStickType = JoyStickType.Flexible;
            else
                m_JoyStickType = JoyStickType.FlexibleOnOff;

            EventTrigger m_Jtrigger =
                m_JoystickPickPanel.GetComponent<EventTrigger>();

            if (m_Jtrigger != null)
                Destroy(m_Jtrigger);

            Vector3[] v = new Vector3[4];

            m_JoySBackObj.GetComponent<RectTransform>().GetWorldCorners(v);

            m_Radius = v[2].y - v[0].y;
            m_Radius = m_Radius / 3.0f;

            m_OriginPos = m_JStickImg.transform.position;
            m_JoySBackObj.GetComponent<Image>().raycastTarget = false;
            m_JStickImg.raycastTarget = false;

            EventTrigger trigger = m_JoystickPickPanel.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) =>
            {
                OnPointerDown_Flx((PointerEventData)data);
            });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener((data) =>
            {
                OnPointerUp_Flx((PointerEventData)data);
            });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((data) =>
            {
                OnDragFlx_JoyStick((PointerEventData)data);
            });
            trigger.triggers.Add(entry);
        }
        #endregion

      
    }

    void Update()
    {
        CursorOb();
    }

    #region --- Cursor Mark 처리 부분
    public void CurMarkOn(Vector3 a_PPos)
    {
        if (m_CursorMark == null) return;

        m_CursorMark.transform.position =
            new Vector3(a_PPos.x, a_PPos.y + 0.1f, a_PPos.z);

        m_CursorMark.SetActive(true);



    }
    void CursorOb()
    {
        if (m_CursorMark == null || m_CursorMark.activeSelf == false || m_RefHero == null) return;

        m_CacLenV = m_RefHero.transform.position - m_CursorMark.transform.position;

        m_CacLenV.y = 0.0f;

        if (m_CacLenV.magnitude < 1.0f)
            m_CursorMark.SetActive(false);



    }
    #endregion

    #region --- Fixed JoyStick 처리 부분
    public void OnJStickDown()
    {
        if (m_JoySBackObj != null)
            m_JoySBackObj.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        if (m_JStickImg != null)
            m_JStickImg.color = new Color32(255, 255, 255, 255);
    }

    public void OnJStickUp()
    {
        if (m_JoySBackObj != null)
            m_JoySBackObj.GetComponent<Image>().color = new Color32(255, 255, 255, 130);
        if (m_JStickImg != null)
            m_JStickImg.color = new Color32(255, 255, 255, 120);
    }

    void OnDragJoyStick(PointerEventData data)
    {
        if (m_JStickImg == null)
            return;

        m_JsCacVec = Input.mousePosition - m_OriginPos;
        m_JsCacVec.z = 0.0f;
        m_JsCacDist = m_JsCacVec.magnitude;
        m_Axis = m_JsCacVec.normalized;

        //조이스틱 백그라운드를 벗어나지 못하게 막는 부분
        if (m_Radius < m_JsCacDist)
        {
            m_JStickImg.transform.position = m_OriginPos + m_Axis * m_Radius;
        }
        else
        {
            m_JStickImg.transform.position = m_OriginPos + m_Axis * m_JsCacDist;
        }

        if (1.0f < m_JsCacDist)
            m_JsCacDist = 1.0f;

        //캐릭터 이동 처리  
        if (m_RefHero != null)
            m_RefHero.SetJoyStickMv(m_JsCacDist, m_Axis);
    }

    void OnEndDragJoyStick(PointerEventData data)
    {
        if (m_JStickImg == null)
            return;

        m_Axis = Vector3.zero;
        m_JStickImg.transform.position = m_OriginPos;

        m_JsCacDist = 0.0f;

        //캐릭터 이동 처리  
        if (m_RefHero != null)
            m_RefHero.SetJoyStickMv(m_JsCacDist, m_Axis);
    }

    #endregion

    #region # Flexible JoyStick 처리 부분
    void OnPointerDown_Flx(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left)
            return;

        if (m_JoySBackObj == null || m_JStickImg == null)
            return;

        m_JoySBackObj.transform.position = data.position;
        m_JStickImg.transform.position = data.position;

        m_JoySBackObj.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        m_JStickImg.color = new Color32(255, 255, 255, 255);

        m_JoySBackObj.SetActive(true);
    }

    void OnPointerUp_Flx(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left)
            return;

        if (m_JoySBackObj == null || m_JStickImg == null)
            return;

        m_JoySBackObj.transform.position = m_OriginPos;
        m_JStickImg.transform.position = m_OriginPos;

        m_JoySBackObj.GetComponent<Image>().color = new Color32(255, 255, 255, 130);
        m_JStickImg.color = new Color32(255, 255, 255, 120);

        m_JoySBackObj.SetActive(false);

        m_Axis = Vector3.zero;
        m_JsCacDist = 0.0f;

        // 캐릭터 정지
        if (m_RefHero != null)
            m_RefHero.SetJoyStickMv(0, Vector3.zero);
    }

    void OnDragFlx_JoyStick(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left)
            return;

        if (m_JoySBackObj == null || m_JStickImg == null)
            return;

        posJoyBack = (Vector2)m_JoySBackObj.transform.position;
        m_JsCacDist = Vector2.Distance(posJoyBack, data.position);
        dirStick = (Vector3)data.position - posJoyBack;

        if (m_Radius < m_JsCacDist)
        {
            m_JsCacDist = m_Radius;
            m_JStickImg.transform.position =
             (Vector3)(posJoyBack + (dirStick.normalized * m_Radius));
        }
        else
        {
            m_JStickImg.transform.position = (Vector3)data.position;
        }

        if (1.0f < m_JsCacDist)
            m_JsCacDist = 1.0f;

        m_Axis = (Vector3)dirStick.normalized;

        //캐릭터 이동 처리
        if (m_RefHero != null)
            m_RefHero.SetJoyStickMv(m_JsCacDist, m_Axis);
    }
    #endregion

    public static bool IsPointerOverUIObject() //UGUI의 UI들이 먼저 피킹되는지 확인하는 함수
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
    }
}
