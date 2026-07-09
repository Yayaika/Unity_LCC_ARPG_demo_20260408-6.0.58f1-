using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    #region 鏡頭設定
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    [Range(0f, 20f)]
    private float distance;
    [SerializeField]
    [Range(-30f, 80f)]
    private float angX;
    [SerializeField]
    [Range(0f, 360f)]
    private float angY;

    [Header("旋轉速度設定")]
    [SerializeField]
    private float rotateSpeed = 200f; // 控制視角轉動的速度
    #endregion 鏡頭設定

    #region 公用參數
    /// <summary>
    /// 角色定位+偏移修正後的最終位置
    /// </summary>
    private Vector3 GPS => GameManager.playerGPS + offset;
    /// <summary>
    /// 是否取得跟隨目標對象
    /// </summary>
    private bool GotTarget => GPS != Vector3.zero;
    #endregion 公用參數

    #region 生命週期
    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleRotationInput(); // 1. 每幀優先處理並更新視角角度 (angX, angY)
        Follow();
    }
    #endregion 生命週期

    private void Follow()
    {
        if (!GotTarget) return;
        transform.position = GPS + Angle() * Distance();
        //transform.LookAt(GPS);
    }

    /// <summary>
    /// 組合角度
    /// </summary>
    /// <returns>四元素運算結果</returns>
    private Quaternion Angle()
    {
        return transform.rotation =
            Quaternion.Euler(angX, angY, 0);
    }

    /// <summary>
    /// 方向向量(距離)
    /// </summary>
    /// <returns>後退長度</returns>
    private Vector3 Distance()
    {
        return Vector3.back * distance;
    }

    /// <summary>
    /// 讀取新 Input Action 的 Look 軸向並操作視角旋轉
    /// </summary>
    private void HandleRotationInput()
    {
        // 確保操作的玩家角色存在，避免在生成前觸發 NullReferenceException
        if (GameManager.playerCtrl == null) return;

        // 從新綁定的 Look (Vector2) 獲取輸入值 (包含鍵盤箭頭與手柄右搖桿)
        Vector2 lookInput = GameManager.playerCtrl.InputCtrl.Play.Look.ReadValue<Vector2>();

        // 當有輸入時才進行運算
        if (lookInput.sqrMagnitude > 0.001f)
        {
            // 左右旋轉：水平輸入 (lookInput.x) 改變繞 Y 軸的視角 (angY)
            angY += lookInput.x * rotateSpeed * Time.deltaTime;

            // 上下旋轉：垂直輸入 (lookInput.y) 改變繞 X 軸的視角 (angX)
            // 註：減號代表標準操作（方向鍵上/搖桿上推時抬頭），若想反轉改為 += 即可
            angX -= lookInput.y * rotateSpeed * Time.deltaTime;

            // 限制上下視角最大與最小範圍，防止翻轉 (限制與你原本的 Inspector Range 一致)
            angX = Mathf.Clamp(angX, -30f, 80f);

            // 確保 angY 始終保持在 0 ~ 360 度之間循環
            if (angY < 0f) angY += 360f;
            if (angY > 360f) angY -= 360f;
        }
    }
}