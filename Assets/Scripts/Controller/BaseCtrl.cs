using System;
using UnityEngine;

/// <summary>
/// 角色控制器抽象層
/// </summary>
[RequireComponent(typeof(CharacterController))]
public abstract class BaseCtrl : MonoBehaviour
{
    #region 基础元件
    /// <summary>
    /// CharacterController元件本體(盡量不直接控制)
    /// </summary>
    private CharacterController _charCtrl;
    /// <summary>
    /// [延遲載入]CharacterController元件
    /// </summary>
    protected CharacterController charCtrl => _charCtrl ??= GetComponent<CharacterController>();
    /// <summary>
    /// AnimaCtrl元件本體
    /// </summary>
    private AnimaCtrl _animaCtrl;
    /// <summary>
    /// [延遲載入]AnimaCtrl元件
    /// </summary>
    protected AnimaCtrl animaCtrl => _animaCtrl ??= GetComponentInChildren<AnimaCtrl>();

    #endregion 基础元件

    #region 狀態機
    /// <summary>
    /// 狀態機定義
    /// </summary>
    public enum State { Idle, Move, Jump, Dash, Attack, Hit, Dead }
    /// <summary>
    /// 角色當前狀態
    /// </summary>
    protected State state = State.Idle;
    /// <summary>
    /// 切換狀態
    /// </summary>
    /// <param name="state">新狀態</param>
    protected void ChangeState(State state)
    {
        if (this.state == state) return;
        this.state = state;
    }

    protected void StateLogic()
    {
        switch (state)
        {
            case State.Idle:
                if (IsMoving) ChangeState(State.Move);
                if (!IsGrounded) ChangeState(State.Jump); // 下墜(無起跳過程)
                break;
            case State.Move:
                Rota();
                _velocity.z = transform.forward.z * MoveSpeed;
                _velocity.x = transform.forward.x * MoveSpeed;
                if (!IsMoving) ChangeState(State.Idle);
                if (!IsGrounded) ChangeState(State.Jump); // 下墜(無起跳過程)
                break;
            case State.Jump:
                Rota();
                _velocity.z = transform.forward.z * MoveSpeed;
                _velocity.x = transform.forward.x * MoveSpeed;
                if (IsGrounded) ChangeState(IsMoving ? State.Move : State.Idle);
                break;
            case State.Dash:
                // 未實作
                break;
            case State.Attack:
                _velocity.z = 0;
                _velocity.x = 0;
                break;
        }
    }
    #endregion 狀態機

    #region 基本參數
    protected Vector3 _facingVector;
    protected Vector3 _velocity;
    [SerializeField]
    protected float _moveSpeed = 5f;
    [SerializeField]
    protected float _jumpHeight = 3f;
    protected float _jumpPower = 1f;

    protected int _combo;
    protected bool _inComboWindow;
    [SerializeField]
    protected GameObject[] _skillPrefabs;
    #endregion 基本參數

    #region 音效元件
    [Header("基礎音效元件")]
    /// <summary>
    /// 負責播放此角色音效的元件
    /// </summary>
    [SerializeField]
    protected AudioSource _audioSource;

    [SerializeField]
    protected AudioClip _hitSound;  // 受擊通用音效
    [SerializeField]
    protected AudioClip _deadSound; // 死亡通用音效
    #endregion 音效元件

    #region 角色屬性參數
    /// <summary>
    /// 最大生命值
    /// </summary>
    [SerializeField]
    protected float _maxHP = 100f;
    /// <summary>
    /// 當前的生命值
    /// </summary>
    protected float _HP;
    [SerializeField]
    protected float _breakDef;
    public event Action<float, float> OnHPChanged;
    //===== 屬性公用參數 =====
    public float CurrentHP => _HP;
    public float MaxHP => _maxHP;

    public float PercentHP => _HP / _maxHP;
    public bool IsDead => state == State.Dead;
    #endregion 角色屬性參數

    #region 抽象公用屬性參數
    /// <summary>
    /// 取得操作的方向向量
    /// </summary>
    public abstract Vector2 MoveInput { get; }

    /// <summary>
    /// 面向的方向向量
    /// </summary>
    public Vector3 FacingVector
    {
        get
        {
            _facingVector.x = MoveInput.x;
            _facingVector.z = MoveInput.y;
            return _facingVector;
        }
    }
    /// <summary>
    /// 角度補償
    /// </summary>
    public virtual Quaternion AngComp => Quaternion.identity;
    /// <summary>
    /// 依據方向向量輸入判定是否在移動中
    /// </summary>
    public bool IsMoving => MoveInput != Vector2.zero;
    /// <summary>
    /// 是否正在執行攻擊動作
    /// </summary>
    public bool IsAttacking => state == State.Attack;
    /// <summary>
    /// 移動倍率(標準化 0~1)
    /// </summary>
    public float MoveMulti => MoveInput.magnitude;
    /// <summary>
    /// 當前移動可達速度
    /// </summary>
    public float MoveSpeed => MoveInput.magnitude * _moveSpeed;
    /// <summary>
    /// 重力值
    /// </summary>
    public float G => Mathf.Abs(Physics.gravity.y);
    /// <summary>
    /// 當前跳躍可達高度
    /// </summary>
    public float H => _jumpHeight * _jumpPower;
    /// <summary>
    /// 是否處於觸地狀態
    /// </summary>
    public bool IsGrounded => charCtrl.isGrounded && _velocity.y < 0;
    /// <summary>
    /// 用於位移的動能
    /// </summary>
    public Vector3 Velocity => _velocity * Time.deltaTime;
    public float VelocityY => _velocity.y;

    public int Combo
    {
        get => _combo;
        set
        {
            _combo = value;
            if (_combo > 2) _combo = 1;
        }
    }
    public event Action OnDied; // 【新增】死亡事件
    #endregion 抽象公用屬性參數

    #region 生命週期
    protected virtual void Awake()
    {
        _HP = _maxHP;//登場回滿血(初始化)

        // 【新增】如果沒在 Inspector 指定，就嘗試從物件本身抓取 AudioSource
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        // 如果連物件本身都沒有 AudioSource，就自動幫它加一個
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        // 建議預設關閉 Play On Awake，避免生成時發出雜音
        _audioSource.playOnAwake = false;
    }

    /// <summary>
    /// 狀態刷新
    /// </summary>
    protected virtual void Update()
    {
        StateLogic();
        AnimaUpdate();
        Movement();
    }
    /// <summary>
    /// 動畫更新
    /// </summary>
    void AnimaUpdate()
    {
        animaCtrl.SetBool(AniHash.IsMoving, IsMoving);
        animaCtrl.SetBool(AniHash.IsGrounded, IsGrounded);
        animaCtrl.SetBool(AniHash.IsAttacking, IsAttacking);
        animaCtrl.SetBool(AniHash.IsDead, IsDead);
        animaCtrl.SetFloat(AniHash.MoveMulti, MoveMulti);
        animaCtrl.SetFloat(AniHash.VelocityY, VelocityY);
        animaCtrl.SetInteger(AniHash.Combo, Combo);
    }
    #endregion 生命週期

    #region 物理控制
    /// <summary>
    /// 動態套用
    /// </summary>
    protected void Movement()
    {
        Gravity();//重力
        if (charCtrl.enabled) charCtrl.Move(Velocity);
    }
    /// <summary>
    /// 重力
    /// </summary>
    protected virtual void Gravity()
    {
        if (IsGrounded)
        {
            if (IsDead)
            {
                _velocity = Vector3.zero;
                charCtrl.enabled = false;
            }
            else
            {
                _velocity.y = -1f;
                _jumpPower = 1f;
            }
        }
        else if (state != State.Dash)
        {
            _velocity.y -= G * Time.deltaTime;
        }
    }
    /// <summary>
    /// 轉向事件
    /// </summary>
    protected void Rota()
    {//轉向
        if (FacingVector != Vector3.zero)
            charCtrl.transform.rotation = Quaternion.LookRotation(FacingVector) * AngComp;
    }
    #endregion 物理控制

    #region 基礎戰鬥動作
    protected void JumpHandle()
    {
        ChangeState(State.Jump);
        _velocity.y = Mathf.Sqrt(2 * G * H);
        animaCtrl.SetTrigger(AniHash.JumpTrigger);
    }

    protected void AttackHandle()
    {
        ChangeState(State.Attack);
        animaCtrl.SetTrigger(AniHash.AttackTrigger);
    }
    #endregion 基礎戰鬥動作

    #region 受擊與傷害邏輯
    protected virtual void SetOnHPChangedEvent(Action<float, float> action)
    {
        OnHPChanged = action;
        OnHPChanged?.Invoke(CurrentHP, MaxHP);
    }
    /// <summary>
    /// 共通傷害與治療執行接口
    /// </summary>
    /// <param name="damage">傷害值（正數為受傷，負數為回復）</param>
    public virtual void TakeDamage(float damage)
    {
        if (IsDead) return; // 避免鞭屍或死後回血

        if (damage > 0)
        {
            // --- 正常受傷邏輯 ---
            _HP -= damage;
            HitHandle(damage);
            OnHPChanged?.Invoke(CurrentHP, MaxHP);
        }
        else if (damage < 0)
        {
            // --- 【核心新增】安全的治療邏輯：傳入負值代表扣除負值（等於加血） ---
            // 使用 Mathf.Min 確保回血後的 _HP 絕對不會超過 _maxHP
            _HP = Mathf.Min(_HP - damage, _maxHP);

            // 治療成功也必須實時執行數值傳遞，讓 UI 血條同步刷新
            OnHPChanged?.Invoke(CurrentHP, MaxHP);
        }

        if (_HP <= 0) Die();
    }
    /// <summary>
    /// 觸發受傷狀態與動畫
    /// </summary>
    protected virtual void HitHandle(float damage)
    {
        ChangeState(State.Hit);
        _velocity.x = 0;
        _velocity.z = 0;
        if (damage > _breakDef) animaCtrl.SetTrigger(AniHash.HitTrigger);
        // 【新增】在 3D 空間中角色的位置播放受擊音效
        if (_hitSound != null)
        {
            AudioSource.PlayClipAtPoint(_hitSound, transform.position);
        }
    }
    /// <summary>
    /// 觸發死亡狀態與動畫
    /// </summary>
    protected virtual void Die()
    {
        _HP = 0;
        ChangeState(State.Dead);
        //下墜中的物理落地後執行
        //_velocity = Vector3.zero;
        //charCtrl.enabled = false;
        animaCtrl.SetTrigger(AniHash.DeadTrigger);
        OnDied?.Invoke(); // 【新增】觸發死亡通知
                          // 【新增】在 3D 空間中角色的位置播放死亡音效
        if (_deadSound != null)
        {
            AudioSource.PlayClipAtPoint(_deadSound, transform.position);
        }

        // ─── 【全新新增：安全埋點】 ───
        // 判斷當前死掉的物件，是不是玩家自己
        if (GameManager.playerCtrl != null && this.gameObject == GameManager.playerCtrl.gameObject)
        {
            GameManager.AddDeathCount(); // 累計玩家死亡次數
            Debug.Log("[數據統計] 玩家死亡，累計次數 +1");
        }
    }

    public void EndHit()
    {
        if (state == State.Hit)
            ChangeState(IsGrounded ? State.Idle : State.Jump);
    }
    #endregion 受擊與傷害邏輯

    #region 動畫控制取用
    public void StartAttack()
    {
        _inComboWindow = false;
    }
    public void OpenComboWindow()
    {
        _inComboWindow = true;
    }

    public void EndAttack()
    {
        _inComboWindow = false;
        if (state == State.Attack)
        {
            ChangeState(IsGrounded ? State.Idle : State.Jump);
        }
    }

    // 原始版本：給小怪用
    public virtual void OnAttack(Transform point)
    {
        if (_skillPrefabs == null || _skillPrefabs.Length == 0) return;
        // 1. 生成技能
        GameObject skillObj = Instantiate(_skillPrefabs[0], point.position, point.rotation);

        // 2. 獲取發射者與技能的 Collider
        Collider myCol = GetComponent<Collider>();
        Collider skillCol = skillObj.GetComponent<Collider>();

        // 3. 如果兩者都有 Collider，則設定忽略碰撞
        if (myCol != null && skillCol != null)
        {
            Physics.IgnoreCollision(myCol, skillCol);
        }
    }

    // 支援傳入 index 的版本
    public virtual void OnAttack(Transform point, int index)
    {
        if (_skillPrefabs == null || _skillPrefabs.Length == 0) return;

        // 1. 防呆：確保 index 不會超過陣列長度
        int skillIndex = index < _skillPrefabs.Length ? index : 0;

        // 2. 💡 關鍵修復：位置用發射點 (point.position)，但角度「強制使用角色正前方 (transform.rotation)」
        Quaternion correctRotation = transform.rotation;

        // 3. 生成對應 Combo 的技能 Prefab
        GameObject skillObj = Instantiate(_skillPrefabs[skillIndex], point.position, correctRotation);

        // 4. 忽略與自身的物理碰撞
        Collider myCol = GetComponent<Collider>();
        Collider skillCol = skillObj.GetComponent<Collider>();
        if (myCol != null && skillCol != null)
        {
            Physics.IgnoreCollision(myCol, skillCol);
        }
    }


    #endregion 動畫控制取用
}
