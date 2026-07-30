using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableItem : MonoBehaviour
{
    [Header("互動按鍵設定")]
    [SerializeField] private InputActionProperty _interactAction;

    [Header("UI 與 特效")]
    [Tooltip("請在此放入此物件專屬的文字 UI 預製物 (例如：[F] 飲用泉水)")]
    [SerializeField] private InteractableUI _uiController;
    [SerializeField] private float healAmount = 20f;
    [SerializeField] private GameObject healVFX;

    // =================【全新新增：可在物品 Inspector 調整的提示高度】=================
    [Header("提示文字高度微調")]
    [Tooltip("此物品專用的提示高度。")]
    [SerializeField] private Vector3 _uiOffset = new Vector3(0, 1.5f, 0);
    // =============================================================================

    [Header("音效設定")]
    [SerializeField] private AudioClip _interactSFX;
    [Range(0f, 1f)][SerializeField] private float _sfxVolume = 1.0f;

    [Header("互動後物理設定")]
    [SerializeField] private bool _enablePhysicsAfterInteract = true;
    [SerializeField] private Collider _solidCollider;

    [Header("銷毀設定")]
    [SerializeField] private bool _destroyAfterDelay = false;
    [SerializeField] private float _destroyDelay = 10f;

    private bool _isPlayerInside = false;
    private bool _hasInteracted = false;
    private Rigidbody _rb;
    private Collider _triggerCollider;

    // =================【修改：儲存動態生成出來的 UI 實例】=================
    private InteractableUI _activeUIInstance;
    // ====================================================================

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _triggerCollider = GetComponent<Collider>();

        if (_solidCollider == null)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                {
                    _solidCollider = col;
                    break;
                }
            }
        }

        if (_rb != null)
        {
            _rb.isKinematic = true;
        }
    }

    private void OnEnable() => _interactAction.action?.Enable();
    private void OnDisable() => _interactAction.action?.Disable();

    private void Update()
    {
        if (_hasInteracted) return;

        if (_isPlayerInside && _interactAction.action != null && _interactAction.action.WasPressedThisFrame())
        {
            PerformInteraction();
        }
    }

    private void PerformInteraction()
    {
        if (_hasInteracted) return;
        _hasInteracted = true;

        if (GameManager.playerCtrl != null)
        {
            GameManager.playerCtrl.TakeDamage(-healAmount);
        }

        if (healVFX != null) Instantiate(healVFX, transform.position, Quaternion.identity);

        if (_interactSFX != null)
        {
            AudioSource.PlayClipAtPoint(_interactSFX, transform.position, _sfxVolume);
        }

        // =================【修改：互動時讓文字淡出銷毀】=================
        if (_activeUIInstance != null)
        {
            _activeUIInstance.Hide(); // 呼叫後會自動淡出並 Destroy
            _activeUIInstance = null;
        }
        // ==============================================================

        _isPlayerInside = false;

        if (_triggerCollider != null && _triggerCollider.isTrigger)
        {
            _triggerCollider.enabled = false;
        }

        if (_enablePhysicsAfterInteract && _rb != null)
        {
            _rb.isKinematic = false;

            if (_solidCollider != null)
            {
                _solidCollider.enabled = true;
            }
        }

        if (_destroyAfterDelay)
        {
            Destroy(gameObject, _destroyDelay);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasInteracted) return;

        if (GameManager.playerCtrl != null && other.gameObject == GameManager.playerCtrl.gameObject)
        {
            _isPlayerInside = true;

            if (_uiController != null && _activeUIInstance == null)
            {
                GameObject canvasObj = GameObject.Find("ItemUICanvas");
                Transform parentCanvas = canvasObj != null ? canvasObj.transform : null;

                _activeUIInstance = Instantiate(_uiController, parentCanvas);

                // 直接將當前物品 Inspector 中設定的偏移量賦予生成的 UI 實例
                _activeUIInstance.offset = _uiOffset;

                _activeUIInstance.Show(transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_hasInteracted) return;

        if (GameManager.playerCtrl != null && other.gameObject == GameManager.playerCtrl.gameObject)
        {
            _isPlayerInside = false;

            // =================【修改：離開時讓文字淡出並銷毀】=================
            if (_activeUIInstance != null)
            {
                _activeUIInstance.Hide(); // 呼叫後會自動淡出並 Destroy
                _activeUIInstance = null;
            }
            // ==============================================================
        }
    }
}