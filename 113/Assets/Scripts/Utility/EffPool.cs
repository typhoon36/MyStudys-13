using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EffPool : MonoBehaviour
{
    Dictionary<string, List<EffUnit_Pool>> m_Pool = new Dictionary<string, List<EffUnit_Pool>>();

    int m_PoolSize = 3;

    #region Singleton
    public static EffPool Inst;
    void Awake()
    {
        Inst = this;

    }
    #endregion

    void Start()
    {
        StartCreate("FX_Hit_01");
        StartCreate("FX_Attack01_01");
        StartCreate("FX_AttackCritical_01");

    }

    public void StartCreate(string a_EffName)
    {
        List<EffUnit_Pool> ListEffPool = null;

        if (m_Pool.ContainsKey(a_EffName) == true)
        {
            ListEffPool = new List<EffUnit_Pool>();
        }
        else
        {
            m_Pool.Add(a_EffName, ListEffPool);
            ListEffPool = m_Pool[a_EffName];
        }

        GameObject a_obj = Resources.Load<GameObject>("Effect/" + a_EffName);

        if (a_obj != null)
        {
            var results = a_obj.GetComponentsInChildren<Transform>();

            for (int k = 0; k< results.Length; k++)
                results[k].gameObject.layer = LayerMask.NameToLayer("TransparentFX");


            for (int j = 0; j < m_PoolSize; j++)
            {
                GameObject obj = Instantiate(a_obj);

                EffUnit_Pool objPool = obj.GetComponent<EffUnit_Pool>();

                if (objPool == null)
                {
                    objPool = obj.AddComponent<EffUnit_Pool>();
                }

                obj.transform.SetParent(this.transform);
                obj.GetComponent<EffUnit_Pool>().SetObjPool(a_EffName, this);

                if (obj.activeSelf == true)
                {
                    obj.SetActive(false); //비활성화 이벤트 동작
                }
                else
                {
                    AddPool(a_EffName, obj.GetComponent<EffUnit_Pool>());
                }

            }

        }
    }

    public void AddPool(string name, EffUnit_Pool obj)
    {
        List<EffUnit_Pool> ListEffPool = m_Pool[name];
        if (ListEffPool != null)
        {
            ListEffPool.Add(obj);
        }


    }

    public GameObject GetEffObj(string _Name, Vector3 _Pos, Quaternion _Rot)
    {
        List<EffUnit_Pool> _ListObjPool = null;

        if (m_Pool.ContainsKey(_Name) == true)
        {
            _ListObjPool = m_Pool[_Name];
        }
        else
        {
            m_Pool.Add(_Name, new List<EffUnit_Pool>());
            _ListObjPool = m_Pool[_Name];
        }

        if (_ListObjPool == null) return null;

        if (_ListObjPool.Count > 0)
        {
            if (_ListObjPool[0] != null && _ListObjPool[0].IsReady())
            {
                //0번째 오브젝트 검사
                EffUnit_Pool unit = _ListObjPool[0];

                _ListObjPool.Remove(_ListObjPool[0]);

                unit.transform.position = _Pos;
                unit.transform.rotation = _Rot;
                StartCoroutine(SetActiveCo(unit.gameObject));
                return unit.gameObject;
            }
        }

        GameObject a_Prefab = Resources.Load<GameObject>("Effect/" + _Name);
        GameObject a_Obj = Instantiate(a_Prefab) as GameObject;

        EffUnit_Pool ObjPoolUnit = a_Obj.GetComponent<EffUnit_Pool>();

        if (ObjPoolUnit == null)
        {
            ObjPoolUnit = a_Obj.AddComponent<EffUnit_Pool>();


        }



        a_Obj.GetComponent<EffUnit_Pool>().SetObjPool(_Name, this);
        StartCoroutine(SetActiveCo(a_Obj));
        return a_Obj;
    }

    IEnumerator SetActiveCo(GameObject a_Obj)
    {
        yield return new WaitForEndOfFrame();
        a_Obj.SetActive(true);
    }

}
