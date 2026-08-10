using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TeleportPoint : MonoBehaviour
{
    [Header("轉場目標設定")]
    [Tooltip("要切換到的新場景名稱，例如 Stage02")]
    [SerializeField] private string _targetStageName = "Stage02";

    [Tooltip("進入新場景後，玩家出現的世界座標")]
    [SerializeField] private Vector3 _targetSpawnPosition;

    [Header("黑屏加載場景設定")]
    [Tooltip("你的黑屏加載場景名稱 (例如 LoadCover)")]
    [SerializeField] private string _loadCoverSceneName = "LoadCover";

    [Header("互動按鍵與 UI 設定")]
    [Tooltip("請綁定傳送互動按鍵 (例如 Keyboards/E 或 Gamepad/buttonNorth)")]
    [SerializeField] private InputActionProperty _interactAction;

    [Tooltip("請在此放入傳送點專屬的 UI 預製物 (例如：[E] 傳送至新區域)")]
    [SerializeField] private InteractableUI _uiController;

    [Tooltip("此傳送點專用的提示文字高度偏移")]
    [SerializeField] private Vector3 _uiOffset = new Vector3(0, 2.0f, 0);

    private bool _isPlayerInside = false;
    private InteractableUI _activeUIInstance;

    private void OnEnable() => _interactAction.action?.Enable();
    private void OnDisable() => _interactAction.action?.Disable();

    private void Update()
    {
        if (_isPlayerInside && _interactAction.action != null && _interactAction.action.WasPressedThisFrame())
        {
            PerformTeleport();
        }
    }

    private void PerformTeleport()
    {
        _isPlayerInside = false;

        // 1. 隱藏/淡出提示 UI
        if (_activeUIInstance != null)
        {
            _activeUIInstance.Hide(); //[cite: 2]
            _activeUIInstance = null; //[cite: 2]
        }

        // 2. 更新玩家位置 (若有 CharacterController，先關閉防物理干擾)
        if (GameManager.playerCtrl != null)
        {
            CharacterController cc = GameManager.playerCtrl.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            GameManager.playerCtrl.transform.position = _targetSpawnPosition;

            if (cc != null) cc.enabled = true;
        }

        // 3. 卸載當前世界關卡場景 (如 Stage01，避免卸載到 GamingUI)
        string currentSceneName = gameObject.scene.name;
        if (currentSceneName != "GamingUI" && SceneManager.GetSceneByName(currentSceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(currentSceneName);
        }

        // 4. 設定 SceneChanger 靜態變數供 LoadCoverCtrl 讀取[cite: 1]
        SceneChanger.targetSceneName = "GamingUI";
        SceneChanger.additiveSceneName = _targetStageName;

        // 5. 以 Additive 模式加載黑屏場景，其 LoadCoverCtrl 會自動接手後續異步加載流程[cite: 1]
        SceneManager.LoadSceneAsync(_loadCoverSceneName, LoadSceneMode.Additive);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.playerCtrl != null && other.gameObject == GameManager.playerCtrl.gameObject)
        {
            _isPlayerInside = true;

            // 沿用 InteractableItem 的 UI 生成邏輯[cite: 2]
            if (_uiController != null && _activeUIInstance == null)
            {
                GameObject canvasObj = GameObject.Find("ItemUICanvas"); //[cite: 2]
                Transform parentCanvas = canvasObj != null ? canvasObj.transform : null; //[cite: 2]

                _activeUIInstance = Instantiate(_uiController, parentCanvas); //[cite: 2]
                _activeUIInstance.offset = _uiOffset; //[cite: 2]
                _activeUIInstance.Show(transform); //[cite: 2]
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (GameManager.playerCtrl != null && other.gameObject == GameManager.playerCtrl.gameObject)
        {
            _isPlayerInside = false;

            if (_activeUIInstance != null)
            {
                _activeUIInstance.Hide(); //[cite: 2]
                _activeUIInstance = null; //[cite: 2]
            }
        }
    }
}