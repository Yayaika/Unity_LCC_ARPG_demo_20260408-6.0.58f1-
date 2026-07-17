using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(CanvasGroup))]
public class InteractableUI : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private bool _isFading = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f; // 預設隱藏
    }

    private void LateUpdate()
    {
        if (Camera.main != null)
        {
            // Billboard 效果：讓 UI 面向攝影機
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    // 將 async void 改為公用方法，供外部調用
    public void Show()
    {
        _isFading = true; // 設定為顯示狀態
        FadeIn();
    }

    public void Hide()
    {
        _isFading = false; // 設定為隱藏狀態
        FadeOut();
    }

    private async void FadeIn(float duration = 0.3f)
    {
        float elapsed = 0f;
        while (elapsed < duration && _isFading)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            await Task.Yield();
        }
        if (_isFading) _canvasGroup.alpha = 1f;
    }

    private async void FadeOut(float duration = 0.3f)
    {
        float elapsed = 0f;
        while (elapsed < duration && !_isFading)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            await Task.Yield();
        }
        if (!_isFading) _canvasGroup.alpha = 0f;
    }
}