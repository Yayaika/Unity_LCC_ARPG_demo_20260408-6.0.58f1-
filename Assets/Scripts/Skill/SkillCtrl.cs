using UnityEngine;
using Unity.Cinemachine;
using System;

public class SkillCtrl : MonoBehaviour
{
    #region 基礎元建
    /// <summary>
    /// 鏡頭震動元件本體
    /// </summary>
    private CinemachineImpulseSource _impulseSource;
    private CinemachineImpulseSource impulseSource
    {
        get
        {
            if (_impulseSource == null)
            {
                _impulseSource = GetComponent<CinemachineImpulseSource>();
                if (_impulseSource == null)
                    _impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            }
            return _impulseSource;
        }
    }

    #endregion 基礎元建

    #region 基本參數
    /// <summary>
    /// [列舉]目標類型
    /// </summary>
    public enum Target { None, Enemy, Player }
    [SerializeField]
    private Target target;
    /// <summary>
    /// 實際運作的對象目標標籤
    /// </summary>
    private string Tag
    {
        get
        {
            switch (target)
            {
                case Target.Enemy: return "Enemy";
                case Target.Player: return "Player";
            }
            return string.Empty;
        }
    }
    /// <summary>
    /// 撞擊效果
    /// </summary>
    [SerializeField]
    private GameObject _hitEffectObj;
    /// <summary>
    /// 銷毀時間
    /// </summary>
    [SerializeField]
    private float _destroyTime = 2f;
    #endregion 基本參數

    #region 運作方式
    /// <summary>
    /// [鏡頭震動]打擊感的威力
    /// </summary>
    [SerializeField]
    private float _hitPower = 0f;
    /// <summary>
    /// [鏡頭震動]是否啟用
    /// </summary>
    private bool HitShock => _hitPower > 0f;
    /// <summary>
    /// [位移]飛射速度
    /// </summary>
    [SerializeField]
    private float _flySpeed = 0f;
    private float FlySpeed => _flySpeed * Time.deltaTime;
    /// <summary>
    /// [位移]是否啟用
    /// </summary>
    private bool CanFly => _flySpeed > 0f;
    #endregion 運作方式

    #region 生命週期
    void Start()
    {
        Destroy(gameObject, _destroyTime);
    }

    private void Update()
    {
        Fly();
    }
    #endregion 生命週期


    /// <summary>
    /// 物件上必須要有碰撞器，且勾上IsTrigger
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tag))
        {     
            _hitEffectObj.SetActive(true);
            if (HitShock) impulseSource.GenerateImpulseWithForce(_hitPower);
        }
    }

    #region 運行手段
    private void Fly()
    {
        if (!CanFly) return;
        transform.Translate(Vector3.forward * FlySpeed);
    }

    #endregion 運行手段
}
