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
    private GameObject magicDoor; // 門口的空氣牆

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
    // 【關鍵修復】加入狀態鎖，防止重複觸發
    private bool _isBattleActive = false;

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

            // 1. 播放進入房間的短音效
            if (_bgmSource != null && _entranceSFX != null)
            {
                _bgmSource.PlayOneShot(_entranceSFX);
            }

            // 2. 播放進場動畫
            director.Play();

            if (magicDoor != null) magicDoor.SetActive(true);
            if (battleZone != null) battleZone.SetActive(true);
            cinemachineCamera.Priority.Value = 100;

            // 3. 設定延遲：等動畫播完才開始放 Boss BGM
            // director.duration 是 Timeline 的總長度
            Invoke(nameof(StartBossBGM), (float)director.duration);

            _ = bossCtrl.Ready((float)director.duration);
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
        if (_isBattleActive && !bossCtrl.IsDead) return;
        if (other.CompareTag(Tag))
        {
            cinemachineCamera.Priority.Value = 0;
        }
    }

    // 【新增】當玩家在房間內死掉時執行的重置邏輯
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
    }

    // 通用的音樂播放函式
    // 統一停止音樂與重置相機/門的方法
    private void ResetRoomStatus()
    {
        _isBattleActive = false;
        CancelInvoke(nameof(StartBossBGM)); // 防止還沒播完動畫就死掉，結果後來又噴出音樂

        // 停止所有音樂播放
        if (_bgmSource != null) _bgmSource.Stop();

        if (magicDoor != null) magicDoor.SetActive(false);
        if (cinemachineCamera != null) cinemachineCamera.Priority.Value = 0;

        if (_currentPlayer != null) _currentPlayer.OnDied -= OnPlayerDiedInRoom;
    }
    #endregion
}