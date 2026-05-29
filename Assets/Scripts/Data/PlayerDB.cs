using System;
using UnityEngine;

/// <summary>
/// 角色基本資料
/// </summary>
[Serializable]
public struct PlayerData
{
    /// <summary>
    /// 名稱
    /// </summary>
    public string name;
    /// <summary>
    /// 説明描述
    /// </summary>
    [TextArea]
    public string desc;
    /// <summary>
    /// 代表性圖標
    /// </summary>
    public Sprite icon;
}

/// <summary>
/// 角色資料庫
/// </summary>
[CreateAssetMenu(fileName = "PlayerDB", menuName = "DataBase/PlayerDB")]
public class PlayerDB : ScriptableObject
{
    public PlayerData[] datas; // 角色資料陣列

    /// <summary>
    /// 使用索引值回傳角色資料
    /// </summary>
    /// <param name="index"></param>
    /// <returns>角色資料</returns>
    public PlayerData GetPlayerData(int index)
    {
        return datas[index];
    }
}
