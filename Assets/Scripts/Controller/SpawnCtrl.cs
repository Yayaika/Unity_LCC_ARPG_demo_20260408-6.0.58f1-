using UnityEngine;

public class SpawnCtrl : MonoBehaviour
{
    #region 基礎元件
    [SerializeField]
    private PlayerDB _playerDB;
    #endregion 基礎元件

    #region 公用參數
    private int PlayerIndex => GameManager.playerIndex;
    private PlayerCtrl CurrentPlayer => _playerDB.GetPlayerData(PlayerIndex).playerCtrl;
    #endregion 公用參數

    void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        // 情況 A：玩家已經存在（切換關卡、關卡間傳送）
        // 玩家物件保持留在 GamingUI / 原場景，不進行 MoveGameObjectToScene
        if (GameManager.playerCtrl != null)
        {
            GameObject playerObj = GameManager.playerCtrl.gameObject;

            // 1. 關閉 CharacterController 避免瞬移位移被物理引擎拉回
            CharacterController cc = playerObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // 2. 只更新世界座標與面向至目前關卡的 SpawnPoint
            playerObj.transform.position = transform.position;
            playerObj.transform.rotation = transform.rotation;

            // 3. 重新開啟 CharacterController
            if (cc != null) cc.enabled = true;

            return;
        }

        // 情況 B：全新開局（場上完全沒有玩家實體時）才 Instantiate 生成
        Instantiate(CurrentPlayer, transform.position, transform.rotation);
    }
}