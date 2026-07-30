using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class EnemyHPBar : MonoBehaviour
{
    [Header("血條元件")]
    [SerializeField] private Image _hpBarImg;
    [SerializeField] private Image _hpBufferBarImg;

    // =================【修改：序列化高度偏移量，使其在 Inspector 可調】=================
    [Header("位置偏移")]
    [Tooltip("血條浮在小怪頭頂的高度偏移量（預設值）")]
    [SerializeField] private Vector3 _offset = new Vector3(0, 1.5f, 0);
    // ===============================================================================

    // 新增一個屬性（Property），允許外部小怪直接修改這個高度
    public Vector3 Offset
    {
        get => _offset;
        set => _offset = value;
    }
    private Transform _targetTransform;
    private Transform _camTransform;
    private Coroutine _bufferCoroutine;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    // =================【修改：允許外部覆寫，若無則採用 Inspector 預設值】=================
    public void Setup(Transform target)
    {
        _targetTransform = target;

        // =================【超強相機定位防呆】=================
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
            _camTransform = targetCam.transform;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            {
                canvas.worldCamera = targetCam;
            }
        }
    }

    private void LateUpdate()
    {
        if (_targetTransform == null || _camTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        // 使用設定好的高度偏移量
        transform.position = _targetTransform.position + _offset;
        transform.rotation = _camTransform.rotation;
    }

    public void SetVisibility(bool visible)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
        }
    }

    public void UpdateHP(float currentHP, float maxHP)
    {
        float targetFill = currentHP / maxHP;
        if (maxHP <= 0) targetFill = 0f;

        if (targetFill >= _hpBufferBarImg.fillAmount)
        {
            if (_bufferCoroutine != null) StopCoroutine(_bufferCoroutine);
            _hpBarImg.fillAmount = targetFill;
            _hpBufferBarImg.fillAmount = targetFill;
        }
        else
        {
            _hpBarImg.fillAmount = targetFill;

            if (_bufferCoroutine != null) StopCoroutine(_bufferCoroutine);
            _bufferCoroutine = StartCoroutine(DelayBufferBarLerp(targetFill));
        }
    }

    private IEnumerator DelayBufferBarLerp(float targetFill)
    {
        yield return new WaitForSeconds(0.15f);
        float startFill = _hpBufferBarImg.fillAmount;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _hpBufferBarImg.fillAmount = Mathf.Lerp(startFill, targetFill, elapsed / duration);
            yield return null;
        }

        _hpBufferBarImg.fillAmount = targetFill;
        _bufferCoroutine = null;
    }
}