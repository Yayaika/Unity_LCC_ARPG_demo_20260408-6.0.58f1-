using UnityEngine;

[RequireComponent(typeof(Animator))]    
public class AnimaCtrl : MonoBehaviour
{
    #region 基础元件
    /// <summary>
    /// Animator元件本體(盡量不直接控制)
    /// </summary>
    private Animator _animaCtrl;
    /// <summary>
    /// [延遲載入]Animator元件
    /// </summary>
    private Animator animator => _animaCtrl ??= GetComponent<Animator>();
    /// <summary>
    /// 角色控制器元件本體
    /// </summary>
    private BaseCtrl _baseCtrl;
    /// <summary>
    /// [延遲載入]角色控制器元件
    /// </summary>
    private BaseCtrl baseCtrl => _baseCtrl ??= GetComponentInParent<BaseCtrl>();
    #endregion 基础元件

    #region 動畫事件資訊
    [SerializeField]
    private Transform[] _eventPoints;
    #endregion 動畫事件資訊

    void Start()
    {
        
    }
    #region 動畫系統基本方法
    /// <summary>
    /// 設定圖層權重
    /// </summary>
    /// <param name="index">圖層序列號</param>
    /// <param name="weight">權重值</param>
    public void SetLayerWeight(int index, float weight) => animator.SetLayerWeight(index, weight);
    /// <summary>
    /// 設置動畫觸發
    /// </summary>
    /// <param name="name"></param>
    public void SetTrigger(int hash) => animator.SetTrigger(hash);


    /// <summary>
    /// 設置動畫布林
    /// </summary>
    /// <param name="name">名稱</param>
    /// <param name="val">值</param>
    public void SetBool(int hash, bool val) => animator.SetBool(hash, val);

    /// <summary>
    /// 設置動畫小數
    /// </summary>
    /// <param name="name">名稱</param>
    /// <param name="val">值</param>
    public void SetFloat(int hash, float val)
    {
        // 檢查該參數是否存在於 Animator 中
        // 注意：這會消耗一點點效能，但在開發階段非常有用
        if (HasParameter(hash))
        {
            animator.SetFloat(hash, val);
        }
    }
    // 新增一個檢查參數是否存在的方法
    private bool HasParameter(int hash)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.nameHash == hash) return true;
        }
        return false;
    }

    /// <summary>
    /// 設置動畫整數
    /// </summary>
    /// <param name="name">名稱</param>
    /// <param name="val">值</param>
    public void SetInteger(int hash, int val) => animator.SetInteger(hash, val);
    #endregion 動畫系統基本方法

    #region 動畫觸發事件
    public void StartAttack() => baseCtrl?.StartAttack();
    public void OnAttack(int index) 
    {
        if (_eventPoints.Length <= 0) return;

        // 防呆：如果傳進來的 index 大於發射點的數量，就預設用第0個
        int pointIndex = index < _eventPoints.Length ? index : 0;
        // 【修改】將 index 同時傳給 BaseCtrl，讓它知道該生成哪一招
        baseCtrl?.OnAttack(_eventPoints[pointIndex], index); 

    }

    public void EndAttack()
    {
        baseCtrl?.EndAttack();
    }

    public void OpenComboWindow()
    {
        baseCtrl?.OpenComboWindow();
    }

    public void EndHit() => baseCtrl?.EndHit();
    #endregion 動畫觸發事件
}

/// <summary>
/// 動作HASH碼清單
/// </summary>
public static class AniHash
{
    public static readonly int IsMoving = Animator.StringToHash("IsMoving");
    public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    public static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    public static readonly int IsDead = Animator.StringToHash("IsDead");

    public static readonly int JumpTrigger = Animator.StringToHash("JumpTrigger");
    public static readonly int DashTrigger = Animator.StringToHash("DashTrigger");
    public static readonly int AttackTrigger = Animator.StringToHash("AttackTrigger");
    public static readonly int HitTrigger = Animator.StringToHash("HitTrigger");
    public static readonly int DeadTrigger = Animator.StringToHash("DeadTrigger");
    public static readonly int RoarTrigger = Animator.StringToHash("RoarTrigger");

    public static readonly int DashBlend = Animator.StringToHash("DashBlend");
    public static readonly int VelocityY = Animator.StringToHash("VelocityY");
    public static readonly int MoveMulti = Animator.StringToHash("MoveMulti");
    public static readonly int Combo = Animator.StringToHash("Combo");
}