using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISettingPanel : UIPanelCtrl
{
    [Header("金手指與設定")]
    [SerializeField] private Toggle _dashToggle;
    [SerializeField] private Toggle _godModeToggle;
    [SerializeField] private Slider _volumeSlider;

    [Header("數據統計 UI 欄位")]
    [SerializeField] private TextMeshProUGUI _killCountText;
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private TextMeshProUGUI _deathCountText;

    // 當面板開啟時，同步當前設定到 UI 上
    private void OnEnable()
    {
        // 使用 SetIsOnWithoutNotify：這會更新 UI 的勾選狀態，但「不會」觸發下方的事件函數
        if (_dashToggle) _dashToggle.SetIsOnWithoutNotify(SettingsManager.Instance.InfiniteDash);
        if (_godModeToggle) _godModeToggle.SetIsOnWithoutNotify(SettingsManager.Instance.GodMode);
        if (_volumeSlider) _volumeSlider.SetValueWithoutNotify(SettingsManager.Instance.Volume);
        // ------------------ 【新增】資訊統計面板即時刷新 ------------------
        // 1. 面板每次被打開時，立刻刷新一次當前的歷史數據
        RefreshStatsUI();

        // 2. 訂閱 GameManager 的數據變更事件，讓面板開著時數據也能實時跳動
        GameManager.OnStatsChanged += RefreshStatsUI;
    }

    // 當面板關閉時
    private void OnDisable()
    {
        // 3. 【新增】面板關閉時必須取消訂閱，防止記憶體殘留報錯
        GameManager.OnStatsChanged -= RefreshStatsUI;
    }

    /// <summary>
    /// 【新增】專門負責刷新統計文字的方法，格式經過四捨五入優化
    /// </summary>
    private void RefreshStatsUI()
    {
        if (_killCountText != null)
            _killCountText.text = $"擊殺敵人數量: {GameManager.KillCount} 隻";

        if (_damageText != null)
            _damageText.text = $"造成總傷害量: {GameManager.TotalDamageDealt:F0}"; // :F0 代表四捨五入去掉小數點

        if (_deathCountText != null)
            _deathCountText.text = $"玩家死亡次數: {GameManager.DeathCount} 次";
    }

    // 統一由 Inspector 的 OnValueChanged 呼叫這些方法
    public void OnDashToggleChanged(bool val)
    {
        // 如果傳入的值跟當前存的值一樣，就直接跳出，不要重複寫入
        if (SettingsManager.Instance.InfiniteDash == val) return;
        SettingsManager.Instance.InfiniteDash = val;
        Debug.Log($"[UI] Dash 設定已切換為: {val}");
    }

    public void OnGodModeToggleChanged(bool val)
    {
        SettingsManager.Instance.GodMode = val;
    }

    public void OnVolumeSliderChanged(float value)
    {
        // 直接存入數值，SettingsManager 內的 SetVolume 會處理 AudioListener
        SettingsManager.Instance.SetVolume(value);
    }

    /// <summary>
    /// 點擊按鈕切換至 PlayerSetting 場景的方法 (已加入黑屏轉場)
    /// </summary>
    public void OnPlayerSettingClicked()
    {
        // 1. 如果您的設定面板打開時會暫停遊戲（Time.timeScale = 0），切換場景前必須先將時間流速恢復正常
        //    這樣黑屏 Fade 淡入協程 (Time.deltaTime) 才跑得了！
        Time.timeScale = 1f;

        // 2. 呼叫 SceneChanger 的黑屏轉場 (Single 模式載入 "PlayerSetting"，並自動清空 GamingUI 與 遊戲世界)
        SceneChanger.LoadWithCover("PlayerSetting");
    }

    // 關閉與退出邏輯
    public void OnBackClicked() { Switch(false); Time.timeScale = 1f; }
    public void OnExitClicked() { Application.Quit(); }
}