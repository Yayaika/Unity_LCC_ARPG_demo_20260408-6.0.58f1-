using UnityEngine;
using UnityEngine.EventSystems; // 1. 引入命名空間
using UnityEngine.UI;           // 引入 UI 命名空間

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

    [Header("手柄預設聚焦選項")]
    [Tooltip("開啟此面板時預設選取的 UI 元件 (例如 ExitPanel 裡的 NO 按鈕)")]
    [SerializeField] private Selectable _firstSelectable;

    [Tooltip("關閉此面板後，要將焦點歸還給背景的哪個 UI 元件 (例如背景的 EXIT 按鈕，可留空)")]
    [SerializeField] private Selectable _previousSelectable;

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
        if (EventSystem.current == null) return;

        if (B)
        {
            // 面板【開啟】時：將焦點轉移至面板內的預設按鈕 (如 NO 按鈕)
            if (_firstSelectable != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_firstSelectable.gameObject);
            }
        }
        else
        {
            // 面板【關閉】時：將焦點歸還給原本觸發它的背景按鈕 (如背景 EXIT 按鈕)
            if (_previousSelectable != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_previousSelectable.gameObject);
            }
        }
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
