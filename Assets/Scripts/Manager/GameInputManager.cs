using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // 【新增】引入事件系統命名空間

public class GameInputManager : MonoBehaviour
{
    public UISettingPanel settingPanel; // 將 SettingPanel 拖入此欄位

    [Header("手柄預設選中元件")]
    [SerializeField] private GameObject _firstSelectedUI; // 【新增】拖入開啟面板時，手柄第一個要選中的 UI (例如 Dash Toggle)

    private Controls _controls;

    private void Awake()
    {
        _controls = new Controls();
    }

    private void OnEnable()
    {
        _controls.Play.Enable();
        _controls.Play.ToggleMenu.performed += OnToggleMenuPressed;
    }

    private void OnDisable()
    {
        _controls.Play.ToggleMenu.performed -= OnToggleMenuPressed;
        _controls.Play.Disable();
    }

    private void OnToggleMenuPressed(InputAction.CallbackContext context)
    {
        if (settingPanel == null) return;

        bool isOpen = settingPanel.GetComponent<CanvasGroup>().alpha > 0;

        // 切換面板顯示狀態
        settingPanel.Switch(!isOpen);

        // 暫停或恢復遊戲時間
        Time.timeScale = isOpen ? 1f : 0f;

        // 切換滑鼠游標顯示
        Cursor.visible = !isOpen;
        Cursor.lockState = !isOpen ? CursorLockMode.None : CursorLockMode.Locked;

        // 【關鍵新增】處理手柄/鍵盤 UI 導航焦點
        if (!isOpen) // 當開啟面板時 (isOpen 原本是 false，!isOpen 變 true)
        {
            if (_firstSelectedUI != null)
            {
                // 清除當前所有選中焦點，並重新設定焦點到指定 UI 上
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_firstSelectedUI);
            }
        }
        else // 當關閉面板時
        {
            // 清除選中焦點，防手柄殘留 UI 操作狀態
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}