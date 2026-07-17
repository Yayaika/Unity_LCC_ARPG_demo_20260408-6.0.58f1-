using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCtrl : BaseCtrl
{

    #region 基礎参数
    private Controls _controls;

    [SerializeField]
    private int _airJumpCountMax = 1;
    private float _airJumpCount;
    [SerializeField]
    private float _dashSpeed = 16f;
    private float _dashDuration = 0.2f;
    [SerializeField]
    private int _dashCountMax = 1; // 衝刺最大次數 (在 Inspector 設定)
    private int _dashCount;        // 當前剩餘衝刺次數

    [Header("玩家動作音效")]
    [SerializeField] private AudioClip _jumpSound;    // 跳躍聲
    [SerializeField] private AudioClip _dashSound;    // 衝刺聲
    [SerializeField] private AudioClip _respawnSound; // 復活聲

    #endregion 基礎参数

    #region 公用参数
    /// <summary>
    /// 產生一組預設好的控制檔
    /// </summary>
    public Controls InputCtrl => _controls ??= new Controls();
    /// <summary>
    /// 從輸入取得的方向向量
    /// </summary>
    public override Vector2 MoveInput => InputCtrl.Play.Move.ReadValue<Vector2>();

    /// <summary>
    /// 角度補償(攝影機側轉量)
    /// </summary>
    public override Quaternion AngComp
    {
        get
        {
            return Quaternion.Euler(0f, GameManager.mainCameraRota.y, 0f);
        }
    }

    /// <summary>
    /// 是否可以執行空中跳躍
    /// </summary>
    public bool CanAirJump => _airJumpCount > 0;
    #endregion 公用参数

    #region 復活參數
    [Header("復活設定")]

    [SerializeField]
    private GameObject _respawnVFX;     // 復活特效預製體
    [SerializeField]
    private float _respawnDelay = 2f;   // 死亡後躺在地上的等待時間

    // 【新增】自動記錄的出生點座標與旋轉，不需要在 Inspector 拖曳！
    private Vector3 _initialSpawnPos;
    private Quaternion _initialSpawnRot;
    #endregion 復活參數

    #region 生命周期
    // 【新增這整個 Awake 方法】
    protected override void Awake()
    {
        // 1. 呼叫父類別的 Awake，確保玩家生成時會執行 _HP = _maxHP (回滿血)
        base.Awake();

        // 2. 記錄剛生成時的出生點座標與面向角度
        _initialSpawnPos = transform.position;
        _initialSpawnRot = transform.rotation;
    }
    private void OnEnable()
    {
        GameManager.SetCurrentPlayer(this);
        SetOnHPChangedEvent(GameManager.UpdatePlayerHPBar);
        InputCtrl.Play.Enable();
        //操作行爲事件訂閲
        InputCtrl.Play.Jump.performed += Jump;
        InputCtrl.Play.Attack.performed += Attack;
        InputCtrl.Play.Dash.performed += Dash;

    }


    private void OnDisable()
    {
        GameManager.SetCurrentPlayer(null);
        InputCtrl.Play.Disable();
        //操作行爲事件取消訂閲
        InputCtrl.Play.Jump.performed -= Jump;
        InputCtrl.Play.Attack.performed -= Attack;
        InputCtrl.Play.Dash.performed -= Dash;
    }

    void Start()
    {

    }


    /// <summary>
    /// 動畫更新
    /// </summary>
    void AnimaUpdate()
    {
        animaCtrl.SetBool(AniHash.IsMoving, IsMoving);
        animaCtrl.SetBool(AniHash.IsGrounded, IsGrounded);
        animaCtrl.SetBool(AniHash.IsAttacking, IsAttacking);
        animaCtrl.SetFloat(AniHash.MoveMulti, MoveMulti);
        animaCtrl.SetFloat(AniHash.VelocityY, VelocityY);
        animaCtrl.SetInteger(AniHash.Combo, Combo);

        // 【手動修改】將字串 "DashBlend" 改為 Hash 變數 AniHash.DashBlend
        animaCtrl.SetFloat(AniHash.DashBlend, IsGrounded ? 0f : 1f);
    }
    #endregion 生命周期

    #region 角色物理控制 
    /// <summary>
    /// 重力，增强角色跳躍
    /// </summary>
    protected override void Gravity()
    {
        base.Gravity();
        if (IsGrounded)
        {
            _airJumpCount = _airJumpCountMax;
            _dashCount = _dashCountMax; // 【新增】觸地時重置衝刺次數
        }
    }

    #endregion 角色物理控制

    #region 跳躍功能
    /// <summary>
    /// 跳躍事件
    /// </summary>
    /// <param name="context">接收輸入</param>
    void Jump(InputAction.CallbackContext context)
    {
        // 【新增修正】如果已經死亡，直接無視跳躍請求
        if (IsDead) return;
        if (state == State.Attack || state == State.Dash) return;

        if (IsGrounded || CanAirJump)
        {
            // --- 【新增邏輯】只要有跳躍動作，就恢復衝刺次數 ---
            _dashCount = _dashCountMax;
            // 【新增】播放跳躍音效
            // 原代碼：if (_audioSource && _jumpSound) _audioSource.PlayOneShot(_jumpSound);
            // 修改為：
            if (_jumpSound != null)
            {
                AudioSource.PlayClipAtPoint(_jumpSound, transform.position);
            }
            if (IsGrounded) JumpHandle();
            else
            {
                _airJumpCount--;
                _jumpPower = 0.5f;
                JumpHandle();
            }
        }
    }


    #endregion 跳躍功能

    #region 攻擊功能
    private void Attack(InputAction.CallbackContext context)
    {
        // 【新增修正】如果已經死亡，直接無視衝刺請求
        if (IsDead) return;
        if (state == State.Dash) return;

        if (IsAttacking && _inComboWindow)
        {
            Combo++;
            AttackHandle();
            _inComboWindow = false;
        }
        else if (!IsAttacking)
        {//完全停止攻擊后：連續重啓
            Combo = 1;
            AttackHandle();
        }
    }
    #endregion 攻擊功能

    #region 衝刺功能

    private void Dash(InputAction.CallbackContext context)
    {
        // 輸出當前所有狀態，這樣您在 Console 就能直接看懂為何無法衝刺
        Debug.Log($"[Dash Debug] 狀態: {state} | 無限衝刺: {SettingsManager.Instance.InfiniteDash} | 剩餘次數: {_dashCount}");

        if (IsDead) return;

        // 檢查狀態是否卡在攻擊中
        if (state == State.Attack)
        {
            Debug.Log("衝刺失敗：正在攻擊中");
            return;
        }

        if (state == State.Dash)
        {
            Debug.Log("衝刺失敗：已經在衝刺了");
            return;
        }

        // 核心判斷：如果不開無限衝刺 且 次數用完，才禁止
        if (!SettingsManager.Instance.InfiniteDash && _dashCount <= 0)
        {
            Debug.Log("衝刺失敗：沒次數了，且未開啟無限衝刺");
            return;
        }

        // 執行衝刺前扣除次數 (如果是無限模式則不扣)
        if (!SettingsManager.Instance.InfiniteDash)
        {
            _dashCount--;
            Debug.Log($"衝刺成功，剩餘次數: {_dashCount}");
        }
        else
        {
            Debug.Log("衝刺成功 (無限模式)");
        }

        // 【手動修改】將字串 "DashBlend" 改為 Hash 變數 AniHash.DashBlend
        animaCtrl.SetFloat(AniHash.DashBlend, IsGrounded ? 0f : 1f);

        ChangeState(State.Dash);
        animaCtrl.SetTrigger(AniHash.DashTrigger);
        _ = DashHandle();
    }

    private async Task DashHandle()
    {
        // 1. 播放衝刺音效
        // 原代碼：if (_audioSource && _dashSound) _audioSource.PlayOneShot(_dashSound);
        // 修改為：
        if (_dashSound != null)
        {
            AudioSource.PlayClipAtPoint(_dashSound, transform.position);
        }

        // 2. 獲取 CharacterController (這是 Unity 內建組件)
        var controller = GetComponent<CharacterController>();
        if (controller == null) return;

        // 3. 設定初始速度
        transform.rotation = Quaternion.LookRotation(transform.forward);
        _velocity = transform.forward * _dashSpeed;
        _velocity.y = 0;

        // 4. 等待時間 (增加 try-catch 以捕捉潛在崩潰)
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(_dashDuration));
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("衝刺延遲中斷: " + e.Message);
            return;
        }

        // 5. 【關鍵修改】：檢查狀態是否還是 Dash，如果是才歸零並切換
        // 如果期間變成了 Dead 或其他狀態，則不執行這段歸零動作
        if (state == State.Dash)
        {
            _velocity = Vector3.zero;
            ChangeState(IsGrounded ? State.Idle : State.Jump);
        }
    }
    #endregion 衝刺功能

    #region 玩家死亡與復活
    protected override void Die()
    {
        base.Die(); // 先執行 BaseCtrl 的死亡邏輯 (切換Dead狀態、播死亡動畫、扣血)
                    // 在這裡直接加上累計
        //GameManager.AddDeathCount();
        Debug.Log("[數據統計] 玩家觸發覆寫死亡，累計次數 +1");
        _ = RespawnHandle(); // 啟動非同步的復活程序
    }

    private async Task RespawnHandle()
    {
        // 1. 等待死亡動畫播完
        await Task.Delay(TimeSpan.FromSeconds(_respawnDelay));

        // 2. 恢復滿血、切換狀態
        _HP = MaxHP;
        ChangeState(State.Idle);

        // 3. 關閉控制器 -> 瞬移回出生的原點 -> 重新開啟控制器
        charCtrl.enabled = false;
        transform.position = _initialSpawnPos;
        transform.rotation = _initialSpawnRot;
        charCtrl.enabled = true;
        // 【新增】播放復活音效
        // 原代碼：if (_audioSource && _respawnSound) _audioSource.PlayOneShot(_respawnSound);
        // 修改為：
        if (_respawnSound != null)
        {
            AudioSource.PlayClipAtPoint(_respawnSound, transform.position);
        }

        // 4. 播放復活特效
        if (_respawnVFX != null)
        {
            Instantiate(_respawnVFX, transform.position, transform.rotation);
        }
        // 5. 更新 UI 血條
        GameManager.UpdatePlayerHPBar(CurrentHP, MaxHP);
    }
    public override void TakeDamage(float damage)
    {
        // 直接訪問 SettingsManager 檢查狀態
        if (SettingsManager.Instance.GodMode)
        {
            Debug.Log("無敵模式開啟，無視傷害");
            return;
        }
        base.TakeDamage(damage);
    }

    #endregion
}
