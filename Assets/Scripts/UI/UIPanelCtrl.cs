using UnityEngine;
// 預設必須的原件
[RequireComponent(typeof(CanvasGroup))]
public class UIPanelCtrl : MonoBehaviour
{
    #region 基本元件
    /// <summary>
    /// CanvasGroup元件本體(盡量不直接控制)
    /// </summary>
    private CanvasGroup _canvasGroup;
    /// <summary>
    /// [延遲載入]CanvasGroup元件
    /// </summary>
    private CanvasGroup canvasGroup  => _canvasGroup ??= GetComponent<CanvasGroup>();
    #endregion 基本元件

    [Tooltip("UI面板預設是否開啓")]
    public bool openOnAwake;

    void Start()
    {
        Switch(openOnAwake);
    }

    /// <summary>
    /// UI面板切換開關
    /// </summary>
    /// <param name="B">true 開 / false 関</param>
    public void Switch(bool B)
    {
        canvasGroup.alpha = B ? 1 : 0;
        canvasGroup.blocksRaycasts = B;
    }

    /// <summary>
    /// 供退出面板的「確定」按鈕呼叫
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        // 如果是在 Unity 編輯器測試，點擊時停止播放模式
        UnityEditor.EditorApplication.isPaused = true;
#else
        // 正式打包出來的遊戲，執行關閉程式
        Application.Quit();
#endif
    }

    #region ContextMenu測試功能
    [ContextMenu("面板打開")]
    public void PanelOn()
    {
        Switch(true);
    }
    [ContextMenu("面板關閉")]
    public void PanelOff()
    {
        Switch(false);
    }
    #endregion ContextMenu測試功能
}
