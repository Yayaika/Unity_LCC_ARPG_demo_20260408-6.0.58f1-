using UnityEngine;
using Unity.Cinemachine;
using System;
using System.Threading.Tasks;

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
    /// 基本傷害
    /// </summary>
    [SerializeField]
    private float _damage = 10f;

    /// <summary>
    /// 撞擊效果
    /// </summary>
    [SerializeField]
    private GameObject _hitEffectObj;
    /// <summary>
    /// [二段]次級效果
    /// </summary>
    [SerializeField]
    private GameObject _secondHitObj;
    /// <summary>
    /// [二段]觸發延遲
    /// </summary>
    [SerializeField]
    private float _secondHitDelay = 0f;
    /// <summary>
    /// 銷毀時間
    /// </summary>
    [SerializeField]
    private float _destroyTime = 2f;
    [SerializeField]
    private bool _useTargetPos = true;
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
    /// <summary>
    /// 是否有二段打擊
    /// </summary>
    private bool UseSecondHit => _secondHitObj != null;
    /// <summary>
    /// 是否延遲觸發二段打
    /// </summary>
    private bool DelaySecondHit => _secondHitDelay > 0f;

    [Header("技能音效")] // 【新增】
    [SerializeField]
    private AudioClip _launchSound; // 【新增】發射/飛行的聲音 (沒打中時只會聽到這個)
    [SerializeField]
    private AudioClip _hitSound; // 命中時的音效
    private AudioSource _localAudioSource; // 本地的音源元件
    #endregion 運作方式

    #region 生命週期
    void Start()
    {
        _localAudioSource = GetComponent<AudioSource>();

        // 1. 遊戲開始播放發射/飛行音效
        if (_localAudioSource != null && _launchSound != null)
        {
            _localAudioSource.clip = _launchSound;
            _localAudioSource.playOnAwake = false; // 透過腳本精準控制
            _localAudioSource.Play();
        }
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
            BaseCtrl actor = other.GetComponent<BaseCtrl>();
            if (!actor) return;
            actor.TakeDamage(_damage);
            SceondHit(_useTargetPos ? other.transform : transform);

            if(_hitEffectObj) _hitEffectObj?.SetActive(true);
            if (HitShock) impulseSource.GenerateImpulseWithForce(_hitPower);
            // 【新增】播放命中音效
            if (_hitSound != null && _localAudioSource != null)
            {
                // 先停止原本飛行的聲音 (如果有需要的話)
                _localAudioSource.Stop();
                // 使用 PlayClipAtPoint 是最安全的，因為技能本身可能隨後會被 Destroy
                // 這樣聲音會在命中座標點產生一個獨立的臨時音源，播完自動銷毀
                _localAudioSource.PlayOneShot(_hitSound);
                // 3. 如果你的技能會立即 Destroy，請將 Destroy 延後 0.2 秒讓聲音播完
                // CancelInvoke("DestroySelf"); // 假設你有自定義銷毀
            }
        }
    }

    #region 運行手段
    private void Fly()
    {
        if (!CanFly) return;
        transform.Translate(Vector3.forward * FlySpeed);
    }

    /// <summary>
    /// 二段傷害
    /// </summary>
    /// <param name="hitTraget">觸發目標</param>
    private void SceondHit(Transform hitTraget)
    {
        if (!UseSecondHit) return;
        if (DelaySecondHit) _ = SceondHitDelay(hitTraget);
        else Instantiate(_secondHitObj, hitTraget.position, Quaternion.identity);
    }
    /// <summary>
    /// 二段傷害(延遲)
    /// </summary>
    /// <param name="hitTraget">觸發目標</param>
    /// <returns></returns>
    private async Task SceondHitDelay(Transform hitTraget)
    {
        await Task.Delay(TimeSpan.FromSeconds(_secondHitDelay));
        Instantiate(_secondHitObj, hitTraget.position, Quaternion.identity);
    }
    #endregion 運行手段
}
