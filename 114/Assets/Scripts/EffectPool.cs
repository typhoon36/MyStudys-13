using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour
{
    Dictionary<string, List<EffectPoolUnit>> m_DcEffectPool =
                            new Dictionary<string, List<EffectPoolUnit>>();

    int m_PresetSize = 3;   //� �ʿ����� ������ �⺻������ 3���� ����� ����

    //--- �̱��� ����
    public static EffectPool Inst = null;

    void Awake()
    {
        Inst = this;
    }
    //--- �̱��� ����

    // Start is called before the first frame update
    void Start()
    {
        StartCreate("FX_Hit_01");
        StartCreate("FX_Attack01_01");
        StartCreate("FX_AttackCritical_01");
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public void StartCreate(string effectName)
    {
        List<EffectPoolUnit> ListObjectPool = null;
        if(m_DcEffectPool.ContainsKey(effectName) == true)
        {
            ListObjectPool = m_DcEffectPool[effectName];
        }
        else
        {
            m_DcEffectPool.Add(effectName, new List<EffectPoolUnit>());
            ListObjectPool = m_DcEffectPool[effectName];
        }

        GameObject a_Prefab = Resources.Load<GameObject>("Effect/" + effectName);
        if(a_Prefab != null)
        {
            var results = a_Prefab.GetComponentsInChildren<Transform>();
            for (int k = 0; k < results.Length; k++)
                results[k].gameObject.layer = LayerMask.NameToLayer("TransparentFX");
            //TransparentFX : flares�� ������ ������ ��ü�� ���� ���̾��,
            //lens flare�� ��ֹ��� ���� �ʴ´�.
            //���� ��� ���� flare�� Default layermask�� ������ �������� �ڿ� �ִٸ�,
            //flare�� ǥ�õ��� ���� ���̴�. �̶� Layer��
            //TransparentFX �� �����ϸ� �� ���̰� �ȴ�.

            for(int j = 0; j < m_PresetSize; j++) //�̸� 3�� ���� ����� ��
            {
                GameObject obj = Instantiate(a_Prefab) as GameObject;

                EffectPoolUnit ObjectPoolUnit = obj.GetComponent<EffectPoolUnit>();
                if(ObjectPoolUnit == null)
                {
                    ObjectPoolUnit = obj.AddComponent<EffectPoolUnit>();
                }

                obj.transform.SetParent(transform);
                obj.GetComponent<EffectPoolUnit>().SetObjectPool(effectName, this);
                if(obj.activeSelf == true)
                {
                    //���� �� ����Ʈ�� Ǯ�� �� �ִ� ���°� �ƴ�
                    //��Ƽ�� ���� OnDisable �̺�Ʈ�� ���۵�
                    obj.SetActive(false);
                }
                else
                {
                    AddPoolUnit(effectName, obj.GetComponent<EffectPoolUnit>());
                }
            }//for (int j = 0; j < m_PresetSize; j++) //�̸� 3�� ���� ����� ��
        }//if (prefab != null)

    }//public void StartCreate(string effectName)

    public void AddPoolUnit(string effectName, EffectPoolUnit unit)
    {
        List<EffectPoolUnit> ListObjectPool = m_DcEffectPool[effectName];
        if(ListObjectPool != null)
        {
            ListObjectPool.Add(unit);
        }
    }

    public GameObject GetEffectObj(string effectName, 
                                    Vector3 position, Quaternion rotation)
    {
        List<EffectPoolUnit> ListObjectPool = null;
        if(m_DcEffectPool.ContainsKey(effectName) == true)
        {
            ListObjectPool = m_DcEffectPool[effectName];
        }
        else
        {
            m_DcEffectPool.Add(effectName, new List<EffectPoolUnit>());
            ListObjectPool = m_DcEffectPool[effectName];
        }

        if (ListObjectPool == null)
            return null;

        if(0 < ListObjectPool.Count)
        {
            if (ListObjectPool[0] != null && ListObjectPool[0].IsReady())
            { // 0���� �غ� �ȵǸ� �������� ������ �� �� �ֱ� ������ 0�� �˻�

                EffectPoolUnit unit = ListObjectPool[0];
                ListObjectPool.Remove(ListObjectPool[0]);
                unit.transform.position = position;
                unit.transform.rotation = rotation;
                StartCoroutine(MySetActiveCo(unit.gameObject));
                return unit.gameObject;
            }
        }//if(0 < ListObjectPool.Count)

        GameObject a_Prefab = Resources.Load<GameObject>("Effect/" + effectName);
        GameObject obj = Instantiate(a_Prefab) as GameObject;   

        EffectPoolUnit objectPoolUnit = obj.GetComponent<EffectPoolUnit>();
        if (objectPoolUnit == null)
        {
            objectPoolUnit = obj.AddComponent<EffectPoolUnit>();
            //OnDisable()�� �޸�Ǯ�� ������... 1�� ������ �� ��� �����ϵ���...
        }

        obj.GetComponent<EffectPoolUnit>().SetObjectPool(effectName, this);
        StartCoroutine(MySetActiveCo(obj));
        return obj;

    }//public GameObject GetEffectObj(

    IEnumerator MySetActiveCo(GameObject obj)
    {
        yield return new WaitForEndOfFrame();
        //�������� ������ ó���� �Ŀ� �ڷ�ƾ�� �̾ ����˴ϴ�.
        //���� ȭ���� �׸��� �۾��� ���� �Ŀ� ó���ϰ� ���� ���� ���� �� ���˴ϴ�.
        obj.SetActive(true);
    }

}//public class EffectPool : MonoBehaviour
