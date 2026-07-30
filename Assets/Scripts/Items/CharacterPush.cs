using UnityEngine;

[DisallowMultipleComponent]
public class CharacterPush : MonoBehaviour
{
    [Header("推動物理設定")]
    [Tooltip("推力大小，數值越大，物體被推得越遠、越快")]
    [SerializeField] private float _pushPower = 2.0f;

    [Tooltip("是否只限制在水平方向推動（防止物體被往下壓或往天空飛）")]
    [SerializeField] private bool _horizontalOnly = true;

    [Tooltip("是否只推動非 Kinematic 的動態剛體（建議勾選，防止推動靜態場景）")]
    [SerializeField] private bool _onlyPushDynamic = true;

    #region Character Controller 碰撞事件 (主要適用於：玩家)
    // 當掛載了 Character Controller 的角色撞擊到帶有 Rigidbody 的物體時自動觸發
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // 安全檢查
        if (body == null) return;
        if (_onlyPushDynamic && body.isKinematic) return;

        // 忽略來自上方的向下踩踏力，避免將物體壓入地面
        if (hit.moveDirection.y < -0.3f) return;

        // 計算推力方向
        Vector3 pushDir = hit.moveDirection;
        if (_horizontalOnly)
        {
            pushDir.y = 0;
            pushDir.Normalize();
        }

        // 對物體的碰撞點施加衝量力
        body.AddForceAtPosition(pushDir * _pushPower, hit.point, ForceMode.Impulse);
    }
    #endregion

    #region 普通 Rigidbody / Collider 碰撞事件 (主要適用於：敵人或NPC)
    // 當掛載了普通碰撞器與剛體的角色撞擊到其他剛體時觸發
    private void OnCollisionEnter(Collision collision)
    {
        PushOnPhysicalContact(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        PushOnPhysicalContact(collision);
    }

    private void PushOnPhysicalContact(Collision collision)
    {
        Rigidbody body = collision.collider.attachedRigidbody;

        // 安全檢查
        if (body == null) return;
        if (_onlyPushDynamic && body.isKinematic) return;

        // 獲取接觸點
        ContactPoint contact = collision.contacts[0];

        // 忽略來自上方的擠壓
        if (contact.normal.y > 0.3f) return;

        // 碰撞法線的相反方向即為推動方向
        Vector3 pushDir = -contact.normal;
        if (_horizontalOnly)
        {
            pushDir.y = 0;
            pushDir.Normalize();
        }

        // 對物體施加推力
        body.AddForceAtPosition(pushDir * _pushPower * 0.1f, contact.point, ForceMode.Force);
    }
    #endregion
}