using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum MonType
{
    Skeleton = 0,
    Alien,
    Count
}

public class MonsterCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    PhotonView pv = null;

    public MonType monType;

    //--- Hp �� ǥ��
    float CurHp = 100;
    float MaxHp = 100;
    float NetHp = 100;  //CurHp �߰��
    public Image ImgHpbar;
    //--- Hp �� ǥ��

    [HideInInspector] public int m_SpawnIdx = -1; //List<SpawnPos> m_SpawnPos; �� �ε���

    //������ ���� ���� ������ ������ Enum ����
    AnimState m_PreState = AnimState.idle;  //�ִϸ��̼� ������ ���� ����
    [HideInInspector] public AnimState m_CurState = AnimState.idle;  //�ִϸ��̼� ������ ���� ����

    AnimState MonState   = AnimState.idle;  //������ ���� AI ���¸� ������ Enum ����
    //AnimSupporter.cs �ʿ� ���ǵǾ� ����

    //�ν����ͺ信 ǥ���� �ִϸ��̼� Ŭ���� ����
    public Anim anim;   //AnimSupporter.cs �ʿ� ���ǵǾ� ����
    Animation m_RefAnimation = null;    //Skeleton
    Animator m_RefAnimator = null;      //Alien

    //--- Monster AI
    [HideInInspector] public GameObject m_AggroTarget = null;   //������ ���
    int m_AggroTgID = -1;   //�� ���Ͱ� �����ؾ� �� ĳ������ ������ȣ
    Vector3 m_MoveDir = Vector3.zero;   //���� ���� ��� ���� ����
    Vector3 m_CacVLen = Vector3.zero;   //���ΰ��� ���ϴ� ����
    float   m_CacDist = 0.0f;           //�Ÿ� ���� ����
    float   m_TraceDist = 7.0f;         //���� �Ÿ�
    float   m_AttackDist = 1.8f;        //���� �Ÿ�
    Quaternion m_TargetRot;             //ȸ�� ���� ����
    float   m_RotSpeed = 7.0f;          //�ʴ� ȸ�� �ӵ�
    Vector3 m_MoveNextStep = Vector3.zero; //�̵� ���� ����
    float   m_MoveVelocity = 2.0f;      //��� �ʴ� �̵� �ӵ�
    //--- Monster AI

    //--- ��Ʈ�� ����ȭ�� ���Ͽ�...
    Vector3 currPos = Vector3.zero;
    Quaternion currRot = Quaternion.identity;
    int currAnim = 0;
    string m_OldAnim = "";
    //--- ��Ʈ�� ����ȭ�� ���Ͽ�...

    void Awake()
    {
        pv = GetComponent<PhotonView>();

        //���� ĳ������ ��ġ �� ȸ�� ���� ó���� ������ �ʱⰪ ���� 
        currPos = transform.position;
        currRot = transform.rotation;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_RefAnimation = GetComponentInChildren<Animation>();
        m_RefAnimator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //������ Ÿ�ֿ̹� ���� �������� ���÷��� ���� �������
        //LoadScene()�� ���÷��� �ʰ� ȣ��Ǵ� ���� �ذằ
        if (PhotonNetwork.CurrentRoom == null)
            return;

        if (m_CurState == AnimState.die)  //�׾�����...
            return;

        if (pv.IsMine == true)
        {   //���⼭�� pv.IsMine�� InstantiateRoomObject �� ���� �Ǿ��� ������ 
            //PhotonNetwork.IsMasterClient �� �����
            MonStateUpdate();
            MonActionUpdate();
        }
        else
        {
            Remote_TrUpdate();
            Remote_TakeDamage();
            Remote_Animation();
        }
    }

    void MonStateUpdate()
    {
        if(m_AggroTarget == null)  //��׷� Ÿ���� ���� ���� ���� ���
        {
            GameObject[] a_Players = GameObject.FindGameObjectsWithTag("Player");
            for(int i = 0; i < a_Players.Length; i++)
            {
                m_CacVLen = a_Players[i].transform.position - this.transform.position;
                m_CacVLen.y = 0.0f;
                m_MoveDir = m_CacVLen.normalized;   //���ΰ��� ���ϴ� ����
                m_CacDist = m_CacVLen.magnitude;    //���ΰ������� �Ÿ�

                if(m_CacDist <= m_AttackDist) //���ݰŸ� ���� �̳��� ���Դ��� Ȯ��
                {
                    MonState = AnimState.attack;
                    m_AggroTarget = a_Players[i].gameObject;    //Ÿ�� ����
                    //Player[] players = PhotonNetwork.PlayerList;  //using Photon.Realtime;
                    //players[0].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber
                    //== pv.Owner.ActorNumber;    //�÷��̾��� ������ȣ�� �ϳ� ������ ��
                    //m_AggroTgID = a_Players[i].GetComponent<Hero_Ctrl>().pv.ViewID;
                    m_AggroTgID = a_Players[i].GetComponent<Hero_Ctrl>().pv.Owner.ActorNumber;
                    break;
                }
                else if(m_CacDist <= m_TraceDist) //�����Ÿ� ���� �̳��� ���Դ��� Ȯ��
                {
                    MonState = AnimState.trace;
                    m_AggroTarget = a_Players[i].gameObject;    //Ÿ�� ����
                    //m_AggroTgID = a_Players[i].GetComponent<Hero_Ctrl>().pv.ViewID;
                    m_AggroTgID = a_Players[i].GetComponent<Hero_Ctrl>().pv.Owner.ActorNumber;
                    break;
                }
            }//for(int i = 0; i < a_Players.Length; i++)

            if(m_AggroTarget == null)
            {
                MonState = AnimState.idle;  //������ ���¸� idle ���� ����
                m_AggroTgID = -1;
            }
        }
        else  //if(m_AggroTarget != null)  ��׷� Ÿ���� ������ ���
        {
            m_CacVLen = m_AggroTarget.transform.position - this.transform.position;
            m_CacVLen.y = 0.0f;
            m_MoveDir = m_CacVLen.normalized;   //���ΰ��� ���ϴ� ����
            m_CacDist = m_CacVLen.magnitude;    //���ΰ������� �Ÿ�

            if(m_CacDist <= m_AttackDist)   //���ݰŸ� ���� �̳��� ���Դ��� Ȯ��
            {
                MonState = AnimState.attack;
            }
            else if(m_CacDist <= m_TraceDist) //�����Ÿ� ���� �̳��� ���Դ��� Ȯ�� 
            {
                MonState = AnimState.trace;
            }
            else
            {
                MonState = AnimState.idle;  //������ ���¸� idle ���·� ����
                m_AggroTarget = null;
                m_AggroTgID = -1;
            }
        }//else  //if(m_AggroTarget != null)  ��׷� Ÿ���� ������ ���
    }

    void MonActionUpdate()
    {
        if(m_AggroTarget == null)   //��׷� Ÿ���� �������� ���� ���
        {
            //�ִϸ��̼� ���¸� Idle ���·� ���� ���´�.
            ChangeAnimState(AnimState.idle, 0.12f);
        }
        else //if(m_AggroTarget != null)   //��׷� Ÿ���� ���� �ϴ� ���
        {
            if(MonState == AnimState.attack) //���� ���� �϶�
            {
                if(0.0001f < m_MoveDir.magnitude)
                {
                    m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                                        m_TargetRot, Time.deltaTime * m_RotSpeed);
                }

                ChangeAnimState(AnimState.attack, 0.12f);   //���� �ִϸ��̼� ����
            }
            else if(MonState == AnimState.trace)   //�������� �϶� 
            {
                if (0.0001f < m_MoveDir.magnitude)
                {
                    m_TargetRot = Quaternion.LookRotation(m_MoveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                                        m_TargetRot, Time.deltaTime * m_RotSpeed);
                }

                //���� ���� �ִϸ��̼� ���̸� ���� �ִϰ� ���� ��쿡�� ���� �̵��ϵ���...
                if (IsAttackAnim() == true)
                    return;

                //--- ���� �̵� �ڵ�
                m_MoveNextStep = m_MoveDir * (m_MoveVelocity * Time.deltaTime);
                m_MoveNextStep.y = 0.0f;
                transform.position += m_MoveNextStep;

                ChangeAnimState(AnimState.trace, 0.12f);  //���� �ִϸ��̼� ����
            }
            else if(MonState == AnimState.idle)
            {
                ChangeAnimState(AnimState.idle, 0.12f);    //������ �ִϸ��̼� ����
            }
        }//else //if(m_AggroTarget != null)   //��׷� Ÿ���� ���� �ϴ� ���
    }//void MonActionUpdate()

    //�ִϸ��̼� ���� ���� �޼���
    void ChangeAnimState(AnimState newState, float CrossTime = 0.0f)
    {
        if (m_PreState == newState)
            return;

        if(m_RefAnimation != null)
        {
            string a_strAnim = anim.Idle.name;
            if (newState == AnimState.idle)
                a_strAnim = anim.Idle.name;
            else if(newState == AnimState.trace)
                a_strAnim = anim.Move.name;
            else if(newState == AnimState.attack)
                a_strAnim = anim.Attack1.name;
            else if(newState == AnimState.die)
                a_strAnim = anim.Die.name;

            if (0.0f < CrossTime)
                m_RefAnimation.CrossFade(a_strAnim, CrossTime);
            else
                m_RefAnimation.Play(a_strAnim);

        }//if(m_RefAnimation != null)

        if(m_RefAnimator != null)
        {
            m_RefAnimator.ResetTrigger(m_PreState.ToString()); //������ ����Ǿ� �ִ� Trigger ������ ����

            if (0.0f < CrossTime)
                m_RefAnimator.SetTrigger(newState.ToString());
            else
            {
                string animName = anim.Idle.name;
                if (newState == AnimState.die)
                    animName = anim.Die.name;

                m_RefAnimator.Play(animName, -1, 0);
                //��� -1�� Layer Index, �ڿ� 0�� ó������ �ٽ� ���� �÷��� ��Ű�ڴٴ� �ǹ�
            }
        }//if(m_RefAnimator != null)

        m_PreState = newState;
        m_CurState = newState;
    }//void ChangeAnimState(AnimState newState, float CrossTime = 0.0f)

    //--- ���� �ִ� ���� ����
    float m_CacRate = 0.0f;
    float m_NormalTime = 0.0f;
    //--- ���� �ִ� ���� ����
    bool IsAttackAnim()  //���� �ִϸ��̼� ���� üũ �Լ�
    {

        if (m_RefAnimation != null)
        {
            if (m_RefAnimation.IsPlaying(anim.Attack1.name) == true)
            {
                m_NormalTime = m_RefAnimation[anim.Attack1.name].time
                                / m_RefAnimation[anim.Attack1.name].length;

                // m_RefAnimation["Attack1h1"].time
                // ��� ���� �÷��̰� �ǰ� �ִ����� ���� �ð���
                // m_RefAnimation["Attack1h1"].length
                // �ѵ����� ���� �������� �ð���

                //�Ҽ��� �ѵ����� ������ ���� �Ǿ����� ��� ����
                m_CacRate = m_NormalTime - (int)m_NormalTime;

                //���� �ִϸ��̼� ���κ��� �ƴ� �� (���ݾִϸ޽ó� ���̶�� ��)
                if (m_CacRate < 0.95f)
                    return true;
            }
        }//if (m_RefAnimation != null)

        if(m_RefAnimator != null)
        {
            // ���� �ִϸ��̼� ���� ������ �����ɴϴ� (���̾� �ε����� ���� 0)
            AnimatorStateInfo stateInfo = m_RefAnimator.GetCurrentAnimatorStateInfo(0);

            // ���� ���°� ���� �ִϸ��̼����� Ȯ���մϴ�
            if (stateInfo.IsName(anim.Attack1.name))
            {
                // �ִϸ��̼��� ���൵�� �����ɴϴ� (0���� 1 ����, 1�̸� �ִϸ��̼� �Ϸ�)
                m_NormalTime = stateInfo.normalizedTime % 1.0f;

                // �ִϸ��̼��� ���� ���κ��� �ƴ϶�� (95% �̸� ����)
                if (m_NormalTime < 0.95f)
                    return true;
            }
        }//if(m_RefAnimator != null)

        return false;

    }//public bool IsAttackAnim()

    public void Event_AttHit() //�ִϸ��̼� �̺�Ʈ �Լ��� ȣ��
    {
        if (pv.IsMine == true)
        {

            if (m_AggroTarget == null)
                return;

            Vector3 a_DistVec = m_AggroTarget.transform.position - transform.position;
            float a_CacLen = a_DistVec.magnitude;
            a_DistVec.y = 0.0f;

            //���ݰ��� �ȿ� �ִ� ���
            if (Vector3.Dot(transform.forward, a_DistVec.normalized) < 0.0f)
                return; //90���� �Ѵ� ������ �ִٴ� ��

            //���� ���� �ۿ� �ִ� ���
            if ((m_AttackDist + 1.7f) < a_CacLen)
                return;

            m_AggroTarget.GetComponent<Hero_Ctrl>().TakeDamage(10);

        }//if (pv.IsMine == true)
        else //�ƹ�Ÿ�� ���忡����
        {
            if (m_AggroTgID < 0)
                return;

            GameObject a_AggroTg = null;
            GameObject[] a_Players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < a_Players.Length; i++)
            {
                //if (m_AggroTgID == a_Players[i].GetComponent<Hero_Ctrl>().pv.ViewID)
                if (m_AggroTgID == a_Players[i].GetComponent<Hero_Ctrl>().pv.Owner.ActorNumber)
                {
                    a_AggroTg = a_Players[i];
                    break;
                }
            }//for(int i = 0; i < a_Players.Length; i++)

            if (a_AggroTg == null)
                return;

            Vector3 a_DistVec = a_AggroTg.transform.position - transform.position;
            float a_CacLen = a_DistVec.magnitude;
            a_DistVec.y = 0.0f;

            //���ݰ��� �ȿ� �ִ� ���
            if (Vector3.Dot(transform.forward, a_DistVec.normalized) < 0.0f)
                return; //90���� �Ѵ� ������ �ִٴ� ��

            //���� ���� �ۿ� �ִ� ���
            if ((m_AttackDist + 1.7) < a_CacLen)
                return;

            a_AggroTg.GetComponent<Hero_Ctrl>().TakeDamage(10);
        }

    }//public void Event_AttHit() //�ִϸ��̼� �̺�Ʈ �Լ��� ȣ��

    public void TakeDamage(GameObject a_Attacker, float a_Damage = 10.0f)
    {
        if (CurHp <= 0.0f)
            return;

        if (pv.IsMine == true)  //���� �������� IsMine�� ���� ����ش�. IsMine�� �ƴϸ� ���⸸...
        {
            CurHp -= a_Damage;
            if (CurHp < 0.0f)
                CurHp = 0.0f;

            ImgHpbar.fillAmount = CurHp / MaxHp;
        }

        Vector3 a_CacPos = this.transform.position;
        a_CacPos.y += 2.65f;
        GameMgr.Inst.SpawnDamageText((int)a_Damage, a_CacPos);

        if(pv.IsMine == true) //��� ó���� IsMine �϶���... �������� ����ȭ�� ó����
        if(CurHp <= 0.0f)   //���ó��
        {
            //Destroy(gameObject);
            CreateItem();
            PhotonNetwork.Destroy(this.gameObject); //���� ����ȭ
        }
    }//public void TakeDamage(GameObject a_Attacker, float a_Damage = 10.0f)

    void Remote_TrUpdate()
    {
        if(5.0f < (transform.position - currPos).magnitude)
        {
            transform.position = currPos;
        }
        else
        {
            //���� �÷��̾��� Monster�� ���� ���� ��ġ���� �ε巴�� �̵���Ŵ
            transform.position = Vector3.Lerp(transform.position, currPos, Time.deltaTime * 10.0f);
        }

        //���� �÷��̾��� Monter�� ���� ���� ������ŭ �ε巴�� ȸ����Ŵ
        transform.rotation = Quaternion.Slerp(transform.rotation, currRot, Time.deltaTime * 10.0f);
    }

    void Remote_TakeDamage()  //������ ��ǻ�Ϳ��� Hp ����ȭ �Լ�
    {
        if(0.0f < CurHp)
        {
            CurHp = NetHp;
            ImgHpbar.fillAmount = CurHp / (float)MaxHp;

            if(CurHp <= 0.0f)
            {
                CurHp = 0.0f;
                //��� ���⸸ �� �ش�. 
                //���� ������ PhotonNetwork.Destroy(this.gameObject);  �Լ��� ���� ���� ����ȭ ��
            }//if(CurHp <= 0.0f)
        }//if(0.0f < CurHp)
        else
        {
            CurHp = NetHp;
            ImgHpbar.fillAmount = CurHp / (float)MaxHp;
        }
    }//void Remote_TakeDamage()  //������ ��ǻ�Ϳ��� Hp ����ȭ �Լ�

    void Remote_Animation()  //������ ��ǻ�Ϳ��� �ִϸ��̼� ����ȭ �Լ�
    {
        ChangeAnimState(m_CurState, 0.12f);
    }

    void CreateItem()
    {
        Vector3 a_Hpos = transform.position;
        a_Hpos.y += 0.1f;
        

        PhotonNetwork.InstantiateRoomObject("DiamondItem/DiamondPrefab", a_Hpos, Quaternion.identity,0);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //���� �÷��̾��� ��ġ ���� �۽�
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(CurHp);
            stream.SendNext(m_AggroTgID);

            stream.SendNext((int)m_CurState);
            stream.SendNext(m_SpawnIdx);
        }
        else //���� �÷��̾��� ��ġ ���� ����
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext(); 
            NetHp = (float)stream.ReceiveNext();
            m_AggroTgID = (int)stream.ReceiveNext();

            m_CurState = (AnimState)stream.ReceiveNext();
            m_SpawnIdx = (int)stream.ReceiveNext();
        }
    }//public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)

   
}
