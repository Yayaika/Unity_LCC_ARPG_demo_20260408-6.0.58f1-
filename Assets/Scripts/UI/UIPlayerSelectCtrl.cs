using System;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UIPlayerSelectCtrl;


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
        /// 角色名稱
        /// </summary>
        public string name;
        /// <summary>
        /// 虛擬鏡頭設定
        /// </summary>
        public CinemachineCamera vCam;
        /// <summary>
        /// UI狀態提示(是否選中)
        /// </summary>
        public Toggle toggle;
        /// <summary>
        /// 文字説明()
        /// </summary>
        [TextArea]
        public string desc; // description
        /// <summary>
        /// 角色選項開關切換
        /// </summary>
        /// <param name="B">開/關</param>
        public void Switch(bool B)
        {
            vCam.Priority.Enabled = B;
            toggle.isOn = B;
        }

        public string GetName()
        {
            return name;
        }

        public string GetDesc()
        {
            return desc;
        }
    }

 
    /// <summary>
    /// [陣列] 虛擬鏡頭設定集合物
    /// </summary>
    public CharOption[] charOptions;

    /// <summary>
    /// 初始化
    /// </summary>
    private void Start()
    {
        UpdateInfo();
    }

    private void UpdateInfo()
    {
        // 選中預設第一位角色
        charOptions[index].Switch(true);
        _textName.text = charOptions[index].GetName();
        _textDesc.text = charOptions[index].GetDesc();
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
