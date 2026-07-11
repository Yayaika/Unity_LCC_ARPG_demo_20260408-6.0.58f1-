using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

public class RoomCtrl : MonoBehaviour
{
    #region 基礎元件
    [SerializeField]
    private CinemachineCamera cinemachineCamera;

    [SerializeField]
    private PlayableDirector director; // 進場 Timeline (BossWarmUp)

    [SerializeField]
    private PlayableDirector bossOutDirector; // 【新增】退場 Timeline (BossOut)

    [SerializeField]
    private Collider doorBlock; // 門口的空氣牆

    [SerializeField]
    private BossCtrl bossCtrl;

    [SerializeField]
    private GameObject battleZone; // 【新增】戰鬥區域阻擋 (BattleZone)
    #endregion 基礎元件

    private const string Tag = "Player";

    #region 生命週期與事件訂閱
    private void Start()
    {
        // 訂閱 Boss 死亡事件
        if (bossCtrl != null)
        {
            bossCtrl.OnDefeated += OnBossDefeated;
        }
    }

    private void OnDestroy()
    {
        // 取消訂閱，避免切換場景時發生記憶體洩漏
        if (bossCtrl != null)
        {
            bossCtrl.OnDefeated -= OnBossDefeated;
        }
    }
    #endregion

    #region 觸發邏輯
    private void OnTriggerEnter(Collider other)
    {
        // 如果 Boss 已經死亡，玩家再次經過時不要重播進場
        if (bossCtrl != null && bossCtrl.IsDead) return;

        if (other.CompareTag(Tag))
        {
            director.Play();

            if (doorBlock != null) doorBlock.isTrigger = false; // 變成實體牆，關門

            // 確保戰鬥開始時 BattleZone 是開啟的 (保險機制)
            if (battleZone != null) battleZone.SetActive(true);

            cinemachineCamera.Priority.Value = 100;
            _ = bossCtrl.Ready((float)director.duration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Tag))
        {
            cinemachineCamera.Priority.Value = 0;
        }
    }

    // 當 Boss 死亡時會自動執行的動作
    private void OnBossDefeated()
    {
        // 1. 播放死亡運鏡 Timeline
        if (bossOutDirector != null) bossOutDirector.Play();

        // 2. 解除門口空氣牆 (改回 Trigger 讓玩家可以通行出門)
        if (doorBlock != null) doorBlock.isTrigger = true;

        // 3. 【新增】關閉 BattleZone 物件，解除戰鬥區域限制
        if (battleZone != null) battleZone.SetActive(false);

        // 4. 把房間虛擬攝影機的權重降回 0 (歸還攝影機控制權)
        if (cinemachineCamera != null) cinemachineCamera.Priority.Value = 0;
    }
    #endregion
}