using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;

enum JoyStickType
{
    Fixed = 0,
    Flexible = 1,
    FlexibleOnOff = 2
}

public class GameMgr : MonoBehaviour
{
    PhotonView pv;

    [HideInInspector] public Hero_Ctrl m_RefHero = null;

    //----- CursorMark 처리부분
    public GameObject m_CursorMark = null;
    Vector3 m_CacVLen = Vector3.zero;
    //----- CursorMark 처리부분

    //----- Fixed JoyStick 처리 부분
    JoyStickType m_JoyStickType = JoyStickType.Fixed;
    [Header("--- JoyStick ---")]
    public GameObject m_JoySBackObj = null;
    public Image m_JStickImg = null;
    float m_Radius = 0.0f;
    Vector3 m_OriginPos = Vector3.zero;
    Vector3 m_Axis = Vector3.zero;
    Vector3 m_JsCacVec = Vector3.zero;
    float m_JsCacDist = 0.0f;
    //----- Fixed JoyStick 처리 부분

    //----- Flexible JoyStick 처리 부분
    public GameObject m_JoystickPickPanel = null;
    Vector3 posJoyBack;
    Vector3 dirStick;
    //----- Flexible JoyStick 처리 부분

    [Header("--- Button Handle ---")]
    public Button m_BackBtn = null;
    public Button m_Attack_Btn = null;
    public Button m_Skill_Btn = null;

    [Header("--- Damage Text ---")]
    public Transform m_Damage_Canvas = null;
    public GameObject m_DamagePrefab = null;
    RectTransform CanvasRect;
    Vector2 ScreenPos = Vector2.zero;
    Vector2 WdScPos = Vector2.zero;

    [Header("--- Shader---")]
    public Shader g_AddTexShader = null;
    public Shader g_VertextShader = null;

    //---- 스킬 쿨 타임 적용
    Text m_Skill_Cool_Label = null;
    Image m_Skill_Cool_Mask = null;
    Button m_Sk_UI_Btn = null;
    [HideInInspector] public float m_Skill_CurCool = 0.0f;
    float m_Skill_CoolDur = 7.0f;
    //---- 스킬 쿨 타임 적용

    //싱글턴 패턴을 위한 인스턴스 변수 선언
    public static GameMgr Inst = null;

    void Awake()
    {
        Inst = this;

        //PotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();

        //주인공을 생성하는 함수 호출
        CreateHero();
    }
    //싱글턴 패턴을 위한 인스턴스 변수 선언

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true; 

        //--- Attack Button 처리 코드
        if (m_Attack_Btn != null)
            m_Attack_Btn.onClick.AddListener(() =>
            {
                if (m_RefHero != null)
                    m_RefHero.AttackOrder();
            });

        //------ Skill Button 처리 코드
        m_Skill_CurCool = 0.0f;

        if(m_Skill_Btn != null)
        {
            m_Skill_Btn.onClick.AddListener(() =>
            {
                if (m_RefHero != null)
                    m_RefHero.SkillOrder("RainArrow", ref m_Skill_CoolDur, ref m_Skill_CurCool);
            });

            m_Skill_Cool_Label = m_Skill_Btn.transform.GetComponentInChildren<Text>();
            m_Skill_Cool_Mask = m_Skill_Btn.transform.Find("SkillCoolMask").GetComponent<Image>();  

            m_Sk_UI_Btn = m_Skill_Btn.GetComponent<Button>();                 
        }
        //------ Skill Button 처리 코드

        //--- Fixed JoyStick 처리 부분
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
        //--- Fixed JoyStick 처리 부분
        
        //--- Flexible JoyStick 처리 부분
        if(m_JoystickPickPanel != null && m_JoySBackObj != null
            && m_JStickImg != null
            && m_JoystickPickPanel.activeSelf == true)
        {
            if (m_JoySBackObj.activeSelf == true)
                m_JoyStickType = JoyStickType.Flexible;
            else
                m_JoyStickType = JoyStickType.FlexibleOnOff;

            EventTrigger a_JBTrigger =
                            m_JoySBackObj.GetComponent<EventTrigger>();
            if (a_JBTrigger != null)
                Destroy(a_JBTrigger);
            //조이스틸 빽에 설치되어 있는 이벤트 트리거 컴포넌트를 제거한다.

            Vector3[] v = new Vector3[4];
            m_JoySBackObj.GetComponent<RectTransform>().GetWorldCorners(v);
            m_Radius = v[2].y - v[0].y;
            m_Radius = m_Radius / 3.0f;

            m_OriginPos = m_JStickImg.transform.position;
            m_JoySBackObj.GetComponent<Image>().raycastTarget = false;
            m_JStickImg.raycastTarget = false;

            EventTrigger trigger = m_JoystickPickPanel.GetComponent<EventTrigger>();
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
                OnDragJoyStick_Flx((PointerEventData)data);
            });
            trigger.triggers.Add(entry);
        }

        //--- 몬스터 스폰 메니터 스폰
        if(PhotonNetwork.IsMasterClient == true) //방장, 서버역할을 하는 클라이언트
        {
            MonSpawn_Mgr a_SpwnMgr = GameObject.FindObjectOfType<MonSpawn_Mgr>();
            if (a_SpwnMgr == null)
            {
                PhotonNetwork.InstantiateRoomObject("MonSpawn_Mgr", 
                                               Vector3.zero, Quaternion.identity, 0);
            }//if (a_SpwnMgr == null)
        }//if(PhotonNetwork.IsMasterClient == true) //방장, 서버역할을 하는 클라이언트

    } //void Start()

    // Update is called once per frame
    void Update()
    {
        CurorOffObserver();  //<-- 클릭마크 끄기

        SkillCool_Update();
    }

    public void CursorMarkOn(Vector3 a_PickPos)
    {
        if (m_CursorMark == null)
            return;

        m_CursorMark.transform.position =
            new Vector3(a_PickPos.x, a_PickPos.y + 0.1f, a_PickPos.z);

        m_CursorMark.SetActive(true);
    }

    void CurorOffObserver()  //<-- 클릭마크 끄기
    {
        if(m_CursorMark == null)
            return;

        if(m_CursorMark.activeSelf == false)
            return;

        if (m_RefHero == null)
            return;

        m_CacVLen = m_RefHero.transform.position -
                        m_CursorMark.transform.position;
        m_CacVLen.y = 0.0f;

        if (m_CacVLen.magnitude < 1.0f)
            m_CursorMark.SetActive(false);
    }

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
        if(m_JStickImg != null)
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
        if(m_Radius < m_JsCacDist)
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
        if(m_JStickImg == null)
            return;

        m_Axis = Vector3.zero;
        m_JStickImg.transform.position = m_OriginPos;

        m_JsCacDist = 0.0f;

        //캐릭터 이동 처리  
        if (m_RefHero != null)
            m_RefHero.SetJoyStickMv(m_JsCacDist, m_Axis);
    }

#endregion

#region --- Flexible JoyStick 처리 부분
    void OnPointerDown_Flx(PointerEventData data)
    {
        if(data.button != PointerEventData.InputButton.Left) //마우스 왼쪽 버튼만
            return;

        if(m_JoySBackObj == null)
            return;

        if (m_JStickImg == null)
            return;

        m_JoySBackObj.transform.position = data.position;
        m_JStickImg.transform.position = data.position;

        if (m_JoySBackObj != null)
            m_JoySBackObj.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        if (m_JStickImg != null)
            m_JStickImg.color = new Color32(255, 255, 255, 255);

        m_JoySBackObj.SetActive(true);

    }//void OnPointerDown_Flx(PointerEventData data)

    void OnPointerUp_Flx(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left)   //마우스 왼쪽 버튼만
            return;

        if (m_JoySBackObj == null)
            return;

        if (m_JStickImg == null)
            return;

        m_JoySBackObj.transform.position = m_OriginPos;
        m_JStickImg.transform.position = m_OriginPos;

        if (m_JoySBackObj != null)
            m_JoySBackObj.GetComponent<Image>().color = new Color32(255, 255, 255, 130);
        if (m_JStickImg != null)
            m_JStickImg.color = new Color32(255, 255, 255, 120);

        if (m_JoyStickType == JoyStickType.FlexibleOnOff)
            m_JoySBackObj.SetActive(false);

        m_Axis = Vector3.zero;
        m_JsCacDist = 0.0f;

        //캐릭터 정지 처리
        if (m_RefHero != null)
            m_RefHero.SetJoyStickMv(0.0f, Vector3.zero);
    }

    void OnDragJoyStick_Flx(PointerEventData data)
    {
        if(data.button != PointerEventData.InputButton.Left) //마우스 왼쪽 버튼만
            return;

        if(m_JStickImg == null)
            return;

        posJoyBack = (Vector2)m_JoySBackObj.transform.position;
        //조이스틱 백 그라운드 현재 위치 기준
        m_JsCacDist = Vector2.Distance(posJoyBack, data.position);  //거리
        dirStick = (Vector3)data.position - posJoyBack; //방향

        if(m_Radius < m_JsCacDist)
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

        if(m_RefHero != null)
            m_RefHero.SetJoyStickMv(m_JsCacDist, m_Axis);  //캐릭터 이동 처리
    }
    #endregion

    void SkillCool_Update()
    {
        if(0.0f < m_Skill_CurCool)
        {
            m_Skill_CurCool -= Time.deltaTime;
            m_Skill_Cool_Label.text = ((int)m_Skill_CurCool).ToString();
            m_Skill_Cool_Mask.fillAmount = m_Skill_CurCool / m_Skill_CoolDur;

            if (m_Sk_UI_Btn != null)
                m_Skill_Btn.enabled = false;  //버튼 눌려지지 않게 하기...
        }
        else
        {
            m_Skill_CurCool = 0.0f;
            m_Skill_Cool_Label.text = "";
            m_Skill_Cool_Mask.fillAmount = 0.0f;

            if (m_Sk_UI_Btn != null)
                m_Skill_Btn.enabled = true;  //버튼 눌려지지 않게 하기...
        }
    }

    public void SpawnDamageText(int dmg, Vector3 a_SpPos, int a_ColorIdx = 0)
    {
        if(m_DamagePrefab == null || m_Damage_Canvas == null)
            return;

        GameObject a_DamageObj = (GameObject)Instantiate(m_DamagePrefab);
        a_DamageObj.transform.SetParent(m_Damage_Canvas, false);
        DamageText a_DamageTx = a_DamageObj.GetComponent<DamageText>(); 
        a_DamageTx.m_BaseWdPos = a_SpPos;
        a_DamageTx.m_DamageVal = dmg;

        //초기 위치 잡아 주기 //--- World 좌표를 UGUI 좌표로 환산해 주는 코드
        CanvasRect = m_Damage_Canvas.GetComponent<RectTransform>();
        ScreenPos = Camera.main.WorldToViewportPoint(a_SpPos);
        WdScPos.x = ((ScreenPos.x * CanvasRect.sizeDelta.x) -
                                (CanvasRect.sizeDelta.x * 0.5f));
        WdScPos.y = ((ScreenPos.y * CanvasRect.sizeDelta.y) -
                                (CanvasRect.sizeDelta.y * 0.5f));
        a_DamageObj.GetComponent<RectTransform>().anchoredPosition = WdScPos;
        //--- World 좌표를 UGUI 좌표로 환산해 주는 코드

        if(a_ColorIdx == 1) //주인공 일 때 데미지 텍스트 색 바꾸기...
        {
            Outline a_Outline = a_DamageObj.GetComponentInChildren<Outline>();
            a_Outline.effectColor = new Color32(255, 255, 255, 0);
            a_Outline.enabled = false;

            Text a_RefText = a_DamageObj.GetComponentInChildren<Text>();
            a_RefText.color = new Color32(255, 255, 230, 255);
        }
    }//public void SpawnDamageText(int dmg, Vector3 a_SpPos, int a_ColorIdx = 0)

    void CreateHero()
    {
        Vector3 a_HPos = Vector3.zero;
        Vector3 a_AddPos = Vector3.zero;

        GameObject a_HPosObj = GameObject.Find("HeroSpawnPos");
        if(a_HPosObj != null)
        {
            a_AddPos.x = Random.Range(-5.0f, 5.0f);
            a_AddPos.z = Random.Range(-5.0f, 5.0f);
            a_HPos = a_HPosObj.transform.position + a_AddPos;
        }

        PhotonNetwork.Instantiate("SK_Bei_T_pose", a_HPos, Quaternion.identity, 0);
    }

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
    }//public bool IsPointerOverUIObject() 

}
