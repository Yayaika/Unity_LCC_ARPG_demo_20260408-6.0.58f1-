using UnityEngine;
// 預設必須的原件
[RequireComponent(typeof(CanvasGroup))]
public class UIPanelCtrl : MonoBehaviour
{
    /// <summary>
    /// CanvasGroup元件本體(盡量不直接控制)
    /// </summary>
    private CanvasGroup _canvasGroup;
    /// <summary>
    /// [延遲載入]CanvasGroup元件
    /// </summary>
    private CanvasGroup canvasGroup // => _canvasGroup ??= GetComponent<CanvasGroup>();
    {
        get
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }

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
        if (B)
        {
            canvasGroup.alpha = 1;
        }
        else
        {
            canvasGroup.alpha = 0;
        }
        canvasGroup.blocksRaycasts = B;
    }
}
