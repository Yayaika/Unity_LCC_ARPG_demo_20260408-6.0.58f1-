using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCtrl : MonoBehaviour
{
    #region 基础元件
    /// <summary>
    /// CharacterController元件本體(盡量不直接控制)
    /// </summary>
    private CharacterController _charCtrl;
    /// <summary>
    /// [延遲載入]CharacterController元件
    /// </summary>
    private CharacterController charCtrl => _charCtrl ??= GetComponent<CharacterController>();
    /// <summary>
    /// AnimaCtrl元件本體
    /// </summary>
    private AnimaCtrl _animaCtrl;
    /// <summary>
    /// [延遲載入]AnimaCtrl元件
    /// </summary>
    private AnimaCtrl animaCtrl => _animaCtrl ??= GetComponentInChildren<AnimaCtrl>();
    #endregion 基础元件

    #region 基礎参数
    private Controls _controls;
    private Vector3 _facingVector;
    [SerializeField]
    private float _moveSpeed = 3f;
    [SerializeField]
    private float _jumpHeight = 3f;
    private float _jumpPower = 1f;
    [SerializeField]
    private int _airJumpCountMax = 1;
    private float _airJumpCount;
    private Vector3 _velocity;
    public int _combo;
    private bool _isAttacking;

    #endregion 基礎参数

    #region 公用参数
    /// <summary>
    /// 產生一組預設好的控制檔
    /// </summary>

    public Controls InputCtrl => _controls ??= new Controls();
    /// <summary>
    /// 從輸入取得的方向向量
    /// </summary>
    public Vector2 MoveInput => InputCtrl.Play.Move.ReadValue<Vector2>();
    /// <summary>
    /// 面向方向向量
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
    /// 依據方向向量判斷是否正在移動
    /// </summary>
    public bool IsMoving => MoveInput != Vector2.zero;
    /// <summary>
    /// 移動倍率(標準化 0~1)
    /// </summary>
    public float MoveMulit => MoveInput.magnitude;
    /// <summary>
    /// 當前移動可達速度
    /// </summary>
    public float MoveSpeed => MoveInput.magnitude * _moveSpeed;
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
    /// 是否可以執行空中跳躍
    /// </summary>
    public bool CanAirJump => _airJumpCount > 0;
    /// <summary>
    /// 用於位移的動能
    /// </summary>
    public Vector3 Velocity => _velocity * Time.deltaTime;
    public float VelocityY => _velocity.y;

    public int Combo
    {
        get
        {
            return _combo;
        }
        set
        {
            _combo = value;
            if(_combo > 2) _combo = 1;
        }
    }
    #endregion 公用参数

    #region 生命周期
    private void OnEnable()
    {
        InputCtrl.Play.Enable();
        //操作行爲時間訂閲
        InputCtrl.Play.Jump.performed += Jump;
        InputCtrl.Play.Attack.performed += Attack;

    }

    

    private void OnDisable()
    {
        InputCtrl.Play.Disable();

        InputCtrl.Play.Jump.performed -= Jump;
        InputCtrl.Play.Attack.performed -= Attack;
    }

    void Start()
    {
        
    }


    /// <summary>
    /// 狀態更新
    /// </summary>
    void Update()
    {
        AnimaUpdate();
        Rota();
        Movement();
    }

    /// <summary>
    /// 動畫更新
    /// </summary>
    void AnimaUpdate()
    {
        animaCtrl.SetBool("IsMoving", IsMoving);
        animaCtrl.SetBool("IsGrounded", IsGrounded);
        animaCtrl.SetFloat("MoveMulit", MoveMulit);
        animaCtrl.SetFloat("VelocityY", VelocityY);
        animaCtrl.SetInteger("Combo", Combo);

    }
    #endregion 生命周期

    /// <summary>
    /// 動態套用
    /// </summary>
    void Movement()
    {
        _velocity.z = transform.forward.z * MoveSpeed;
        _velocity.x = transform.forward.x * MoveSpeed;
        //重力
        if (IsGrounded)
        {
            _velocity.y = -1f;
            _airJumpCount = _airJumpCountMax;
            _jumpPower = 1f;
        }
        else
        {
            _velocity.y -= G * Time.deltaTime;
        }
        charCtrl.Move(Velocity);
    }

    /// <summary>
    /// 轉向事件
    /// </summary>
    void Rota()
    {
        if (!IsMoving) return;
        //轉向
        charCtrl.transform.rotation = Quaternion.LookRotation(FacingVector);

    }
    #region 跳躍功能
    /// <summary>
    /// 跳躍事件
    /// </summary>
    /// <param name="context">接收輸入</param>
    void Jump(InputAction.CallbackContext context)
    {
        if (IsGrounded) 
        {
            JumpHandle();
        }
        else if (CanAirJump)
        {
            _airJumpCount--;
            _jumpPower = 0.5f;
            JumpHandle();
        }
    }

    void JumpHandle()
    {
        //向上
        _velocity.y = Mathf.Sqrt(2 * G * H);
        animaCtrl.SetTrigger("JumpTrigger");
    }
    #endregion 跳躍功能


    #region 攻擊功能
    private void Attack(InputAction.CallbackContext context)
    {
        if (!_isAttacking) 
        {//完全停止攻擊后：連續重啓
            Combo = 1;
            animaCtrl.SetTrigger("AttackTrigger");
        }
        
    }

    public void StartAttack()
    {
        _isAttacking = true;
    }

    public void EndAttack()
    {
        _isAttacking = false;
    }
    #endregion 攻擊功能
}
