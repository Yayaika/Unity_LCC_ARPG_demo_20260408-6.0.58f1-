using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 掛載在可互動物件或敵人身上，具備「觸發去重/冷卻」功能的流血腳本
/// </summary>
public class DamageableObject : MonoBehaviour
{
    [Header("血跡特效設定")]
    [Tooltip("請將 VFX Graph 預製體 (Prefab) 拖入此處")]
    [SerializeField] private GameObject _bloodVFXPrefab;

    [Tooltip("VFX 特效物件過多久後自動 Destroy (建議 15 秒)")]
    [SerializeField] private float _vfxDestroyDelay = 15f;

    [Header("防重複觸發機制 (冷卻設定)")]
    [Tooltip("每次受擊後的最小間隔時間 (秒)，避免攻擊特效瞬間連續觸發多次")]
    [SerializeField] private float _hitCooldown = 0.3f;

    [Tooltip("允許觸發血跡的攻擊體積 Tag (若為空則不限制 Tag)")]
    [SerializeField] private string _attackTag = "Weapon";

    private float _lastHitTime = -999f;
    private HashSet<int> _processedAttackers = new HashSet<int>();

    #region 公用 API (供外部腳本直接呼叫)

    /// <summary>
    /// 外部 Raycast 或攻擊判定系統呼叫此方法
    /// </summary>
    public void TakeDamage(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (Time.time - _lastHitTime < _hitCooldown) return;

        _lastHitTime = Time.time;
        SpawnBloodEffect(hitPoint, Quaternion.LookRotation(hitNormal));
    }

    #endregion

    #region 物理碰撞觸發 (自動防重複)

    private void OnTriggerEnter(Collider other)
    {
        // 1. 驗證 Tag
        if (!string.IsNullOrEmpty(_attackTag) && !other.CompareTag(_attackTag)) return;

        // 2. 檢查受擊冷卻時間 (CD)
        if (Time.time - _lastHitTime < _hitCooldown) return;

        // 3. 取得此攻擊物件的唯一 ID (避免同一個攻擊特效元件在生命週期內重複觸發)
        int attackID = other.GetInstanceID();
        if (_processedAttackers.Contains(attackID)) return;

        // 通過驗證，記錄狀態
        _lastHitTime = Time.time;
        _processedAttackers.Add(attackID);

        // 4. 計算最佳碰撞點與方向
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 hitNormal = (transform.position - hitPoint).normalized;

        // 如果計算出的法線接近 0 (說明碰撞體已完全穿透中心)，預設朝向攻擊源方向
        if (hitNormal == Vector3.zero)
        {
            hitNormal = (other.transform.position - transform.position).normalized;
        }

        SpawnBloodEffect(hitPoint, Quaternion.LookRotation(hitNormal));
    }

    // 當攻擊特效離開或銷毀時，清空 ID 記錄
    private void OnTriggerExit(Collider other)
    {
        if (_processedAttackers.Contains(other.GetInstanceID()))
        {
            _processedAttackers.Remove(other.GetInstanceID());
        }
    }

    #endregion

    #region 核心生成邏輯

    private void SpawnBloodEffect(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (_bloodVFXPrefab == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 未指定 Blood VFX Prefab！", this);
            return;
        }

        // 1. 在受擊點生成血跡 VFX
        GameObject bloodInstance = Instantiate(_bloodVFXPrefab, spawnPosition, spawnRotation);

        // 2. 設為受擊物體的子物件 (讓空中噴血跟隨物體移動)
        bloodInstance.transform.SetParent(transform);

        // 3. 自動定時銷毀
        Destroy(bloodInstance, _vfxDestroyDelay);
    }

    #endregion
}