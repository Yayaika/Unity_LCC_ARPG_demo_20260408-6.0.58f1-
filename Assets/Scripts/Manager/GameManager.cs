using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 靜態唯一資料管理脚本
/// </summary>
public static class GameManager
{
    #region 玩家相關資訊
    /// <summary>
    /// 當前正在操作角色的索引號碼
    /// </summary>
    public static int playerIndex;
    /// <summary>
    /// 當前正在操作的角色
    /// </summary>
    public static PlayerCtrl playerCtrl { get; private set; }
    /// <summary>
    /// 當前玩家坐標定位
    /// </summary>
    public static Vector3 playerGPS
    {
        get
        {
            return playerCtrl != null ? playerCtrl.transform.position : Vector3.zero;
        }
    }

    /// <summary>
    /// 設定(初始化)當前操作角色
    /// </summary>
    /// <param name="ctrl">角色控制器</param>
    public static void SetCurrentPlayer(PlayerCtrl ctrl)
    {
        playerCtrl = ctrl;
    }
    /// <summary>
    /// 連結HPBarUI的動作
    /// </summary>
    public static Action<float, float> UpdatePlayerHPBar { get; private set; }
    public static void SetPlayerHPBar(Action<float, float> action)
    {
        UpdatePlayerHPBar += action;
        //玩家已存在的話立刻更新一次
        if (playerCtrl) UpdatePlayerHPBar(playerCtrl.CurrentHP, playerCtrl.MaxHP);
    }
    public static void RemovePlayerHPBar(Action<float, float> action)
    {
        UpdatePlayerHPBar -= action;
    }
    public static void ClearPlayerHPBar()
    {
        UpdatePlayerHPBar = null;
    }
    #endregion 玩家相關資訊

    #region 主攝影機相關
    public static CameraManager mainCamrea { get; private set; }
    public static Vector3 mainCameraRota
    {
        get
        {
            return mainCamrea != null
                ? mainCamrea.transform.rotation.eulerAngles
                : Vector3.zero;
        }
    }
    public static void SetMainCamera(CameraManager main)
    {
        mainCamrea = main;
    }
    #endregion 主攝影機相關

    #region Boss相關資訊
    public static BossCtrl bossCtrl { get; private set; }
    /// <summary>
    /// 連結BossHPBarUI的動作
    /// </summary> 
    public static Action<float, float> UpdateBossHPBar { get; private set; }
    /// <summary>
    /// 設定(初始化)當前BOSS
    /// </summary>
    /// <param name="ctrl">BOSS控制器</param>
    public static void SetCurrentBoss(BossCtrl ctrl)
    {
        bossCtrl = ctrl;
        bossCtrl.OnHPChanged += UpdateBossHPBar;
        UpdateBossHPBar?.Invoke(bossCtrl.CurrentHP, bossCtrl.MaxHP);
    }

    public static void SetBossHPBar(Action<float, float> action)
    {
        UpdateBossHPBar += action;
        if (bossCtrl) UpdateBossHPBar(bossCtrl.CurrentHP, bossCtrl.MaxHP);
    }
    public static void RemoveBossHPBar(Action<float, float> action)
    {
        UpdateBossHPBar -= action;
    }
    #endregion Boss相關資訊

    /// <summary>
    /// 加載場景
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="mode"></param>
    public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(sceneName, mode);
    }

    #region 玩家統計數據系統（修正所有腳本報錯）

    // 1. 儲存數據的靜態變數（唯讀，只能透過下方方法修改）
    public static int KillCount { get; private set; } = 0;
    public static float TotalDamageDealt { get; private set; } = 0f;
    public static int DeathCount { get; private set; } = 0;

    // 2. 當數據變動時，用來通知 UI 面板實時刷新的事件
    public static System.Action OnStatsChanged;

    /// <summary>
    /// 累計擊殺數
    /// </summary>
    public static void AddKillCount()
    {
        KillCount++;
        OnStatsChanged?.Invoke(); // 觸發事件，通知 UI 面板更新文字
    }

    /// <summary>
    /// 累計玩家造成的總傷害
    /// </summary>
    /// <param name="damage">傳入受到的傷害值</param>
    public static void AddDamage(float damage)
    {
        if (damage <= 0) return; // 負數（回血）或 0 傷害不計入統計
        TotalDamageDealt += damage;
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 累計玩家死亡次數
    /// </summary>
    public static void AddDeathCount()
    {
        DeathCount++;
        OnStatsChanged?.Invoke();
    }

    #endregion
}