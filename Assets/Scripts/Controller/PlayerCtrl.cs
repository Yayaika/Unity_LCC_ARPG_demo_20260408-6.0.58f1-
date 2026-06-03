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
    #endregion 基础元件

    #region 基礎参数
    private Controls _controls;
    private Vector3 _facingVector;
    [SerializeField]
    private float _moveSpeed = 2f;
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
    #endregion 公用参数

    #region 生命周期
    private void OnEnable()
    {
        InputCtrl.Play.Enable();
         
    }

    private void OnDisable()
    {
        InputCtrl.Play.Disable();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
    #endregion 生命周期

    void Move()
    {
        if (!IsMoving) return;
        //轉向
        charCtrl.transform.rotation = Quaternion.LookRotation(FacingVector);
        //前進
        charCtrl.Move(transform.forward * _moveSpeed * Time.deltaTime);
    }

    void Jump()
    {

    }
}
