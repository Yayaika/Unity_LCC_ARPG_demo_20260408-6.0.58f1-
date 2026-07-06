using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    #region 鏡頭設定
    [SerializeField, Header("鏡頭設定")]
    private Vector3 offset;
    [SerializeField]
    [Range(0f, 20f)]
    private float distance;
    [SerializeField]
    [Range(10f, 80f)]
    private float angX;
    [SerializeField]
    [Range(0f, 360f)]
    private float angY;
    #endregion 鏡頭設定

    #region 公用參數
    /// <summary>
    /// 角色定位+偏移修正的最終位置
    /// </summary>
    private Vector3 GPS => GameManager.playerGPS + offset; // 取得玩家位置
    /// <summary>
    /// 是否取得跟隨目標對象
    /// </summary>
    private bool GotTarget => GPS != Vector3.zero; // 是否有目標
    #endregion 公用參數

    #region 生命週期
    private void OnEnable()
    {
        //GameManager.SetCurrentCamera(this);
    }

    private void OnDisable()
    {
        //GameManager.SetCurrentCamera(null);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Follow();
    }
    #endregion 生命週期

    private void Follow()
    {
        if (!GotTarget) return; // 如果沒有目標就不執行後續程式碼   
        transform.position = GPS + Angle() * Distance(); // 角度與距離相乘是坐標
        //transform.LookAt(GPS);
    }

    /// <summary>
    /// 組合角度
    /// </summary>
    /// <returns>四元素運算結果</returns>
    private Quaternion Angle()
    {
        return transform.rotation = Quaternion.Euler(angX, angY, 0);

    }
    /// <summary>
    /// 方向向量(距離)
    /// </summary>
    /// <returns>後退長度</returns>
    private Vector3 Distance()
    {
        return Vector3.back * distance;
    }
}
