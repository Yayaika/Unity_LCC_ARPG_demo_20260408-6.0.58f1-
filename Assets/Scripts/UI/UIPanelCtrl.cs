using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanelCtrl : MonoBehaviour
{
    #region 基本元件
    private CanvasGroup _canvasGroup;

    // 安全取得 CanvasGroup，避免空指標與已銷毀物件存取
    protected CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            return _canvasGroup;
        }
    }
    #endregion 基本元件

    [Tooltip("UI面板預設是否開啓")]
    public bool openOnAwake;

    [Header("手柄預設聚焦選項")]
    [SerializeField] private Selectable _firstSelectable;
    [SerializeField] private Selectable _previousSelectable;

    // 檢查物件及元件是否存在[cite: 2]
    public bool IsShow => this != null && canvasGroup != null && canvasGroup.alpha > 0;

    void Start()
    {
        Switch(openOnAwake);
    }

    public void Switch(bool B)
    {
        // 若物件或 CanvasGroup 已被銷毀，直接跳出不執行[cite: 2]
        if (this == null || canvasGroup == null) return;

        canvasGroup.alpha = B ? 1 : 0;
        canvasGroup.blocksRaycasts = B;

        if (EventSystem.current == null) return;

        if (B)
        {
            if (_firstSelectable != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_firstSelectable.gameObject);
            }
        }
        else
        {
            if (_previousSelectable != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_previousSelectable.gameObject);
            }
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
#else
        Application.Quit();
#endif
    }
}