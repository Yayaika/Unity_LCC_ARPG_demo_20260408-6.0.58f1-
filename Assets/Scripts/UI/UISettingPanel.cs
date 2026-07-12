using UnityEngine;
using UnityEngine.UI;

public class UISettingPanel : UIPanelCtrl
{
    [Header("金手指與設定")]
    [SerializeField] private Toggle _dashToggle;
    [SerializeField] private Toggle _godModeToggle;
    [SerializeField] private Slider _volumeSlider;

    // 當面板開啟時，同步當前設定到 UI 上
    private void OnEnable()
    {
        // 使用 SetIsOnWithoutNotify：這會更新 UI 的勾選狀態，但「不會」觸發下方的事件函數
        if (_dashToggle) _dashToggle.SetIsOnWithoutNotify(SettingsManager.Instance.InfiniteDash);
        if (_godModeToggle) _godModeToggle.SetIsOnWithoutNotify(SettingsManager.Instance.GodMode);
        if (_volumeSlider) _volumeSlider.SetValueWithoutNotify(SettingsManager.Instance.Volume);
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

    // 關閉與退出邏輯
    public void OnBackClicked() { Switch(false); Time.timeScale = 1f; }
    public void OnExitClicked() { Application.Quit(); }
}