using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableItem : MonoBehaviour
{
    [Header("互動按鍵設定")]
    [SerializeField] private InputActionProperty _interactAction;

    [Header("UI 與 特效")]
    [SerializeField] private InteractableUI _uiController; // 修改這裡，直接指向 UI 控制腳本
    [SerializeField] private float healAmount = 20f;
    [SerializeField] private GameObject healVFX;

    private bool _isPlayerInside = false;

    private void OnEnable() => _interactAction.action?.Enable();
    private void OnDisable() => _interactAction.action?.Disable();

    private void Update()
    {
        if (_isPlayerInside && _interactAction.action != null && _interactAction.action.WasPressedThisFrame())
        {
            PerformInteraction();
        }
    }

    private void PerformInteraction()
    {
        if (GameManager.playerCtrl != null)
        {
            GameManager.playerCtrl.TakeDamage(-healAmount);
        }

        if (healVFX != null) Instantiate(healVFX, transform.position, Quaternion.identity);

        // 互動後隱藏 UI
        if (_uiController != null) _uiController.Hide();
        _isPlayerInside = false;

        // 如果補血桶是一次性的
        // Destroy(gameObject); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.playerCtrl != null && other.gameObject == GameManager.playerCtrl.gameObject)
        {
            _isPlayerInside = true;
            if (_uiController != null) _uiController.Show(); // 呼叫淡入
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (GameManager.playerCtrl != null && other.gameObject == GameManager.playerCtrl.gameObject)
        {
            _isPlayerInside = false;
            if (_uiController != null) _uiController.Hide(); // 呼叫淡出
        }
    }
}