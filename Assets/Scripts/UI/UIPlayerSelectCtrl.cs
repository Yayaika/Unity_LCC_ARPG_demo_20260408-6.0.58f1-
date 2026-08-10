using System;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UIPlayerSelectCtrl;
using static UnityEditor.FilePathAttribute;
using UnityEngine.EventSystems; // 1. 記得引入 EventSystem 命名空間


public class UIPlayerSelectCtrl : MonoBehaviour
{
    /// <summary>
    /// 索引原始值
    /// </summary>
    private int _index;
    /// <summary>
    /// 索引操作變數
    /// </summary>
    private int index
    {
        get
        {
            return _index;
        }
        set
        {// 小於0特殊處理成尾號，否則取餘數
            _index = value < 0 ? charOptions.Length - 1 : value % charOptions.Length;
        }
    }
    [SerializeField]
    private PlayerDB _playerDB;
    [SerializeField]
    private TextMeshProUGUI _textName;
    [SerializeField]
    private TextMeshProUGUI _textDesc;

    /// <summary>
    /// 角色選項資料結構
    /// </summary>
    [Serializable]
    public struct CharOption
    {
        /// <summary>
        /// 虛擬鏡頭設定
        /// </summary>
        public CinemachineCamera vCam;
        /// <summary>
        /// UI狀態提示(是否選中)
        /// </summary>
        public Toggle toggle;
        /// <summary>
        /// 定位(焦點)
        /// </summary>
        public Transform location;

        /// <summary>
        /// 角色選項開關切換
        /// </summary>
        /// <param name="B">開/關</param>
        public void Switch(bool B)
        {
            vCam.Priority.Enabled = B;
            toggle.isOn = B;
        }

        /// <summary>
        /// 設定Toggle圖案
        /// </summary>
        /// <param name="icon">圖案</param>
        public void SetToggle(Sprite icon)
        {
            (toggle.targetGraphic as Image).sprite = icon;
            (toggle.graphic as Image).sprite = icon;
        }
    }
 
    /// <summary>
    /// [陣列] 虛擬鏡頭設定集合物
    /// </summary>
    public CharOption[] charOptions;

    [Header("UI 按鈕引用（用於手柄預設聚焦）")]
    [SerializeField] private Button _enterStageButton; // 拖入「選中進入遊戲」按鈕

    /// <summary>
    /// 初始化
    /// </summary>
    private void Start()
    {
        for(int i = 0; i < charOptions.Length; i++)
        {// 起始(0)；終點(3)；迭代(1)
            PlayerData data = _playerDB.GetPlayerData(i);
            charOptions[i].SetToggle(_playerDB.GetPlayerData(i).icon);
            //具象化物件(預製物，坐標，旋轉)
            //Instantiate(data.playerCtrl, charOptions[i].location.position, charOptions[i].location.rotation);
            // 【修改部分】：先接收生成出來的角色實例
            PlayerCtrl spawnedPlayer = Instantiate(data.playerCtrl, charOptions[i].location.position, charOptions[i].location.rotation);
            if (spawnedPlayer != null)
            {
                spawnedPlayer.enabled = false;
            }
        }
        // 選中預設第一位角色
        UpdateInfo();

        // 在 Start 的最後設定預設選取的按鈕
        SetDefaultFocus();
    }

    private void SetDefaultFocus()
    {
        if (_enterStageButton != null && EventSystem.current != null)
        {
            // 清空目前選取並重新指定，讓 UI 顯示高亮框
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_enterStageButton.gameObject);
        }
    }

    /// <summary>
    /// 更新選角UI資訊
    /// </summary>
    private void UpdateInfo()
    {
        // 選中預設第一位角色
        charOptions[index].Switch(true);
        _textName.text = _playerDB.GetPlayerData(index).name;
        _textDesc.text = _playerDB.GetPlayerData(index).desc;
    }

    /// <summary>
    /// 進入游戲關卡(舞臺)
    /// </summary>
    public void EnterStage()
    {
        GameManager.playerIndex = index;
        // 2. 指定遊戲世界的目標場景名稱
        SceneChanger.LoadWithCover("GamingUI", "Stage01");

        // 3. 疊加載入轉場黑幕
        //SceneManager.LoadScene("LoadCover", LoadSceneMode.Additive);
        // 2. 緊接著用「疊加模式 (Additive)」加載第一關的場景
        // 備註：這行需要引用過 using UnityEngine.SceneManagement; 
        // 如果你的 GameManager 內部已經有包裝好的 LoadSceneAsync，也可以寫在 GameManager 內部。
    }

    /// <summary>
    /// 下一個角色(鏡頭逆轉)
    /// </summary>
    public void NextPlayer()
    {
        charOptions[index].Switch(false);
        index++;//索引增加
        UpdateInfo();

    }
    /// <summary>
    /// 上一個角色(鏡頭順轉)
    /// </summary>
    public void PrePlayer()
    {
        charOptions[index].Switch(false);
        index--;// 索引減少
        UpdateInfo();
    }
}
