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
        // 0. 安全機制：排除發射者自己或同陣營 (例如 Enemy 射出的子彈不打 Enemy)
        // 如果目前目標是 Enemy，那發射者就是 Player，所以要排除 Player 標籤，反之亦然
        string myOwnTag = (target == Target.Enemy) ? "Player" : "Enemy";
        if (other.CompareTag(myOwnTag) || other.CompareTag("Untagged") && other.name.Contains("Trigger"))
            return; // 遇到自己的陣營或是某些純觸發區域(如 BossZone)就穿透過去

        // 1. 狀況 A：精準打中設定的「敵人/目標」標籤
        if (other.CompareTag(Tag))
        {
            BaseCtrl actor = other.GetComponent<BaseCtrl>();
            if (!actor) return;
            actor.TakeDamage(_damage);
            SceondHit(_useTargetPos ? other.transform : transform);

            if(_hitEffectObj) _hitEffectObj?.SetActive(true);
            if (HitShock) impulseSource.GenerateImpulseWithForce(_hitPower);
            // 【新增】播放命中音效
            PlayHitSoundAndDestroy();
            return;
        }
        // 2. 狀況 B：打到了「非目標」的東西 (例如 Terrain、環境障礙物、牆壁等)
        // 只要不是發射者陣營，撞到任何有物理碰撞的東西就立刻阻擋並消失
        Debug.Log($"[Skill] 技能撞擊到非目標物體: {other.name}，觸發提前銷毀。");

        // 如果打到環境也想生出爆炸特效，可以取消下面這行的註解：
        // if (_hitEffectObj) { _hitEffectObj.transform.position = transform.position; _hitEffectObj.SetActive(true); }

        PlayHitSoundAndDestroy();
    }

    /// <summary>
    /// 播放命中音效並確保安全銷毀物件
    /// </summary>
    private void PlayHitSoundAndDestroy()
    {
        if (_hitSound != null && _localAudioSource != null)
        {
            _localAudioSource.Stop();
            _localAudioSource.PlayOneShot(_hitSound);
        }

        // 關閉 MeshRenderer 與 Collider，讓子彈隱形且失去碰撞，但保留 GameObject 讓音效播完
        var renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer) renderer.enabled = false;

        var collider = GetComponent<Collider>();
        if (collider) collider.enabled = false;

        // 停止飛行
        _flySpeed = 0f;

        // 0.2 秒後徹底銷毀，留時間給 PlayOneShot 播完殘響
        Destroy(gameObject, 0.2f);
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
