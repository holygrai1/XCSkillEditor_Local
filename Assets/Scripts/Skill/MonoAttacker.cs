using Assets.Scripts.Enemy;
using cfg;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace XiaoCao
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Rigidbody))]
    public class MonoAttacker : MonoBehaviour, IAttacker, IMessager
    {
        #region Old
        public uint netId;
        //public bool hasAuthority;
        #endregion

        #region NetWork

        public bool isTruePlayer = false; //�ǿͻ��˿��ƵĽ�ɫ

        [Header("MainSetting")]

        //ģ��
        public AgentModelType agentType;

        public bool isEnableAck => true;




        #endregion
        public Animator animator;
        [SerializeField]
        [Header("Ѫ��λ��")]
        private Transform _topTranform;
        public Transform TopTranform
        {
            get
            {
                if (_topTranform == null) { _topTranform = transform; }
                return _topTranform;
            }
        }
        public GameObject SelfObject { get => transform.gameObject; set => SelfObject = value; }
        public AI AI;

        //[SyncVar(hook = nameof(UpdatePlayData))]
        public PlayerData playerData;

        //[SyncVar]
        public BreakPower breakPower;

        public virtual int Hp { get; set; }

        public virtual int MaxHp { get; set; }

        public virtual int MaxBreakPower { get; set; }

        public virtual int NoBreakPower { get; set; }

        public virtual float Ack { get; }

        public bool IsDie => Hp <= 0;

        public virtual bool IsCanNorSkill { get; }

        [Header("DebugView")]
        public bool IsHideHpBar = false;

        public AgentTag AgentTag;

        public DamageState damageState;

        public XCTimer breakTimer = new XCTimer();
        public XCTimer noDamageTimer = new XCTimer();
        public XCTimer dieTimer = new XCTimer();
        public XCTimer noBreakTimer = new XCTimer(); //ba'ti


        public Action<XCEventsRunner> onSkillFinish;
        public Action<AckInfo> OnDamAciton;

        public virtual void UpdatePlayData(PlayerData old, PlayerData newData)
        {
            PlayerMgr.Instance.UpdatePlayerValue();
        }

        public virtual void InitPlayerData() { }

        ///=> <see cref="PlayerState.OnAckTrigger"/>
        public virtual void OnAckTrigger(Collider other ,AckInfo ackInfo) { }

        public virtual void OnDam(AckInfo ackInfo = default)
        {
            ///=> <see cref="PlayerState.OnDam"/>
        }

        public virtual void AIMoveTo(Vector3 dir, float speed =1, bool is_mMove = true)
        {
            ///=> <see cref="PlayerState.AIMoveTo"/>
        }

        public virtual void SetSlowRoteAnim(bool v)
        {
            ///=> <see cref="PlayerState.SetSlowRoteAnim"/>
        }

        //����
        //seeR���Ӿ���Χ
        //angle:�Ӿ��н� 0-180 ������90��ʾ���ں�
        //hearR:������Χ; ����->û��������Χ,0->����angle�Զ�����������Χ�����ҹ涨seeR>hearR
        public MonoAttacker SearchEnemy(float seeR = 15, float seeAngle = 180, float hearR = 0)
        {
            MonoAttacker res = null;
            float minDis = 9999;
            float tmpdis;
            foreach (var item in PlayerMgr.Instance.MonoAttackerDic.Values)
            {
                if (item.AgentTag != AgentTag && !item.IsDie)
                {
                    var tf = item.transform;
                    bool isFinded = false; 
                    bool isInAngle = IsInRangeAngle(tf, seeAngle, out float curAngle);
                    bool isInRange = IsInRangeDis(tf, seeR, out float curDis);

                    if (isInAngle && isInRange)
                    {
                        isFinded = true;
                    }
                    else if (isInRange && !isInAngle && hearR >=0)
                    {
                        if (hearR == 0)
                        {
                            hearR = seeAngle / 180 * seeR;
                        }
                        //��������
                        isFinded = curDis < hearR;
                    }

                    if (isFinded)
                    {
                        //curAngle �ӽ� 0 , ������ľ���ԽС
                        float reAngleRete = Mathf.Lerp(1, 1.5f, curAngle / 180);

                        tmpdis = curDis* reAngleRete;//�������*�н�Ȩ��
                        //�Ƚ� ѡ������ĵ�λ
                        if (tmpdis > 0 && tmpdis < minDis)
                        {
                            minDis = tmpdis;
                            res = item;
                        }
                    }
                }
            }
            return res;
        }


        /// <param name="tf"></param>
        /// <param name="minAngle">��ǰ��������˷���ļн�0-180,����90�����ʾ������</param>
        /// <param name="curAngle">�����ǰ�н�</param>
        /// <returns></returns>
        public bool IsInRangeAngle(Transform tf, float minAngle, out float curAngle)
        {
            Vector3 dir = tf.position - transform.position;

            float angle = Vector3.Angle(dir, transform.forward);

            curAngle = angle;

            return angle < minAngle;
        }

        public bool IsInRangeDis(Transform tf, float dis, out float curDis)
        {
            curDis = Vector3.Distance(tf.position, transform.position);
            return curDis < dis;
        }


        public virtual void MoveSlowDown(float lerp = 0.1f)
        {

        }

        public virtual void SetBool(string name, bool msg)
        {

        }

        public virtual bool SetAIEvent(ActMsgType name, string msg, object other = null)
        {
            ///=> <see cref="PlayerState.SetAIEvent"/>
            return true;
        }

        public virtual void SendAll(string name, float num, bool isOn, string str)
        {
            ///=> <see cref="PlayerState.SendAll"/>
        }
    }

    public class XCTimer
    {
        public string name;
        public float exitTime;
        public float timer;
        public float cdRate = 1;
        public Action action;
        public bool isRuning = true;
        public bool isLoop = false;//ʱ�䵽�Զ�������ʱ ��ѭ��, ѭ����ҪResetTimer()

        public float FillAmount
        {
            get
            {
                if (TotalTime == 0)
                {
                    return 0;
                }

                return Mathf.Min(1, timer / TotalTime);
            }
        }
        public float TotalTime => exitTime * cdRate;

        public XCTimer() { }

        public void Init(string name, float exitTime, Action action)
        {
            this.name = name;
            this.exitTime = exitTime;
            this.action = action;
        }


        public void Update()
        {
            if (isLoop || (!isLoop && isRuning))
            {
                timer += Time.deltaTime;

                if (timer > TotalTime)
                {
                    Exit();
                    isRuning = false;
                }
            }
        }

        public void Exit()
        {
            timer = 0;
            action?.Invoke();
        }

        public void ResetTimer()
        {
            timer = 0;
            isRuning = true;
        }

        public void AddMinTime(float addTime, bool isForce = false)
        {
            isRuning = true;
            if (isForce)
            {
                exitTime = addTime;
                timer = 0;
            }
            else
            {
                //��� ׷��ʱ�� ���� ʣ��ʱ�� ,���ʣ��ʱ�� ����Ϊ׷��ʱ�� ->��������
                var resTime = exitTime - timer;
                if (addTime > resTime)
                {
                    timer = exitTime - addTime;
                }
            }

        }
    }

    public class SkillCDTimer
    {
        public void Init(List<SkillKey> skillKeyList)
        {
            skillCDDic.Clear();
            foreach (var item in skillKeyList)
            {
                XCTimer timer = new XCTimer();
                timer.exitTime = item.cdTime;
                timer.timer = item.cdTime;
                timer.isRuning = false;
                timer.name = item.skillId;
                skillCDDic.Add(item.skillId, timer);
            }
        }
        public void SetCdRate(float cdRate)
        {
            foreach (var item in skillCDDic)
            {
                item.Value.cdRate = cdRate;
            }
        }

        public XCTimer GetTimer(string skillid)
        {
            skillCDDic.TryGetValue(skillid, out XCTimer timer);
            return timer;
        }

        public bool IsCDEnd(string skillid)
        {
            var timer = GetTimer(skillid);
            if (timer != null)
            {
                return !timer.isRuning;
            }
            return false;
        }

        public void ReCountCD(string skillid)
        {
            var timer = GetTimer(skillid);
            if (timer != null)
            {
                timer.ResetTimer();
            }
        }

        public Dictionary<string, XCTimer> skillCDDic = new Dictionary<string, XCTimer>();

        public void OnUpdate()
        {
            foreach (var item in skillCDDic.Values)
            {
                item.Update();
            }
        }
    }

}
