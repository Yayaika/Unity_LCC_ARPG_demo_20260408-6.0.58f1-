using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

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
    private GameObject magicDoor; // 門口的空氣牆 (入口門)

    [SerializeField]
    private GameObject exitMagicDoor; // 【新增】Boss身後的出口空氣牆 (Cube (3))

    [SerializeField]
    private BossCtrl bossCtrl;

    [SerializeField]
    private GameObject battleZone; // 【新增】戰鬥區域阻擋 (BattleZone)
    #endregion 基礎元件

    [Header("音效設定")]
    [SerializeField] private AudioSource _bgmSource; // 這裡要拉入場景中帶有 AudioSource 的物件(喇叭)
    [SerializeField] private AudioClip _entranceSFX;   // 【新增】一般的 MP3 音樂(唱片)
    [SerializeField] private AudioClip _bossBGM;     // Boss 的 MP3 音樂(唱片)

    private const string Tag = "Player";
    private PlayerCtrl _currentPlayer; // 暫存進入房間的玩家

    // 狀態鎖，防止單次戰鬥中重複觸發
    private bool _isBattleActive = false;

    // 【關鍵新增】紀錄這場遊戲中，是否已經播放過 Boss 的登場動畫
    private bool _hasPlayedIntro = false;

    #region 生命週期與事件訂閱
    private void Start()
    {
        // 訂閱 Boss 死亡事件
        if (bossCtrl != null)
        {
            bossCtrl.OnDefeated += OnBossDefeated;
        }

        // 確保遊戲一開始出口空氣牆是擋住的 (SetActive(true))
        if (exitMagicDoor != null)
        {
            exitMagicDoor.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        // 取消訂閱，避免切換場景時發生記憶體洩漏
        if (bossCtrl != null)
        {
            bossCtrl.OnDefeated -= OnBossDefeated;
        }
        // 額外保險：取消訂閱玩家事件
        if (_currentPlayer != null)
        {
            _currentPlayer.OnDied -= OnPlayerDiedInRoom;
        }
    }
    #endregion

    #region 觸發邏輯
    private void OnTriggerEnter(Collider other)
    {
        // 如果 Boss 已經死亡，玩家再次經過時不要重播進場
        if (bossCtrl != null && bossCtrl.IsDead) return;
        if (_isBattleActive) return;

        if (other.CompareTag(Tag))
        {
            _isBattleActive = true;
            _currentPlayer = other.GetComponent<PlayerCtrl>();
            if (_currentPlayer != null)
            {
                _currentPlayer.OnDied -= OnPlayerDiedInRoom;
                _currentPlayer.OnDied += OnPlayerDiedInRoom;
            }

            // 進入房間時封鎖入口門與戰鬥區域
            if (magicDoor != null) magicDoor.SetActive(true);
            if (battleZone != null) battleZone.SetActive(true);
            if (cinemachineCamera != null) cinemachineCamera.Priority.Value = 100;

            // 【邏輯分流】檢查是否已經播過登場動畫
            if (!_hasPlayedIntro)
            {
                // --- 情況 A：第一次進入房間，播放完整登場動畫 ---
                _hasPlayedIntro = true; // 標記為已播放過

                // 1. 播放進入房間的短音效
                if (_bgmSource != null && _entranceSFX != null)
                {
                    _bgmSource.PlayOneShot(_entranceSFX);
                }

                // 2. 播放進場動畫
                if (director != null)
                {
                    director.Play();
                    // 3. 設定延遲：等動畫播完才開始放 Boss BGM
                    Invoke(nameof(StartBossBGM), (float)director.duration);
                    _ = bossCtrl.Ready((float)director.duration);
                }
                else
                {
                    // 防呆：若沒拉 Timeline，直接開打
                    StartBossBGM();
                    _ = bossCtrl.Ready(0f);
                }
            }
            else
            {
                // --- 情況 B：非第一次進入（例如死亡重跑），直接開打 ---
                Debug.Log("[RoomCtrl] 檢測到玩家再次進入，跳過登場動畫直接開打。");

                // 1. 直接播放 Boss 戰鬥 BGM
                StartBossBGM();

                // 2. 讓 Boss 讀取進入準備狀態（0 秒延遲）
                _ = bossCtrl.Ready(0f);
            }
        }
    }

    // 動畫結束後正式開始播 BGM
    private void StartBossBGM()
    {
        if (_isBattleActive && _bgmSource != null && _bossBGM != null)
        {
            _bgmSource.clip = _bossBGM;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Tag))
        {
            Debug.Log("[RoomCtrl] 玩家離開 Boss 區域，關閉鏡頭權重並重置房間狀態。");
            if (cinemachineCamera != null) cinemachineCamera.Priority.Value = 0;

            // 玩家離開時重置戰鬥狀態，但不會重置 _hasPlayedIntro
            ResetRoomStatus();
        }
    }

    // 當玩家在房間內死掉時執行的重置邏輯
    private void OnPlayerDiedInRoom()
    {
        ResetRoomStatus();
    }

    // 當 Boss 死亡時會自動執行的動作
    private void OnBossDefeated()
    {
        if (bossOutDirector != null) bossOutDirector.Play();
        ResetRoomStatus();
        if (battleZone != null) battleZone.SetActive(false);

        // 【關鍵】只有 Boss 死亡時，才關閉出口空氣牆 (Cube (3)) 讓玩家通過
        if (exitMagicDoor != null) exitMagicDoor.SetActive(false);
    }

    // 統一停止音樂與重置相機/門的方法（此處刻意不重置 _hasPlayedIntro）
    private void ResetRoomStatus()
    {
        _isBattleActive = false;
        CancelInvoke(nameof(StartBossBGM)); // 防止還沒播完動畫就死掉，結果後來又噴出音樂

        // 停止所有音樂播放
        if (_bgmSource != null) _bgmSource.Stop();

        // 玩家死亡或離開時，將入口門 (magicDoor) 打開
        if (magicDoor != null) magicDoor.SetActive(false);
        if (cinemachineCamera != null) cinemachineCamera.Priority.Value = 0;
        if (_currentPlayer != null)
        {
            _currentPlayer.OnDied -= OnPlayerDiedInRoom;
            _currentPlayer = null; // 清空引用
        }
    }
    #endregion
}