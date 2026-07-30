using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(CanvasGroup))]
public class InteractableUI : MonoBehaviour
{
    [Header("World Space 跟隨與朝向設定")]
    [Tooltip("物體的跟隨目標")]
    public Transform targetTransform;

    [Tooltip("攝影機的朝向")]
    public Transform camTransform;

    [Tooltip("高度偏移量 (讓文字浮在模型頭頂)")]
    [SerializeField]
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    private CanvasGroup _canvasGroup;
    private bool _isFading = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f; // 預設隱藏
    }

    private void Start()
    {
        // 移除原先在 Start 裡對 targetTransform 的自動判定，避免干擾實例化邏輯

        // 僅保留自動尋找 3D 相機的邏輯
        if (camTransform == null)
        {
            Camera targetCam = Camera.main;
            if (targetCam == null || targetCam.name.Contains("UI") || targetCam.orthographic)
            {
                targetCam = null;
                Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                foreach (Camera cam in allCameras)
                {
                    if (!cam.name.Contains("UI") && !cam.orthographic)
                    {
                        targetCam = cam;
                        break;
                    }
                }
            }

            if (targetCam != null)
            {
                camTransform = targetCam.transform;
                Canvas canvas = GetComponent<Canvas>();
                if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
                {
                    canvas.worldCamera = targetCam;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (targetTransform == null || camTransform == null) return;

        // 1. 位置對齊並加上偏移
        transform.position = targetTransform.position + offset;

        // 2. 旋轉對齊攝影機
        transform.rotation = camTransform.rotation;
    }



    // 供物件初始化位置
    public void Show(Transform target)
    {
        targetTransform = target;
        Show();
    }

    public void Show()
    {
        _isFading = true;
        FadeIn();
    }

    // =================【修改：隱藏時觸發自我銷毀】=================
    public void Hide()
    {
        _isFading = false;
        FadeOutAndDestroy(); // 改為淡出並銷毀
    }

    private async void FadeOutAndDestroy(float duration = 0.3f)
    {
        float elapsed = 0f;
        while (elapsed < duration && !_isFading)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            await Task.Yield();
        }
        if (!_isFading)
        {
            _canvasGroup.alpha = 0f;
            Destroy(gameObject); // 【核心】淡出完成後，將自己從場景中徹底刪除
        }
    }
    // =============================================================

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
}