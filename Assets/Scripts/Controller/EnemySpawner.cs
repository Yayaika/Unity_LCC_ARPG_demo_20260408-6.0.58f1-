using UnityEngine;
using UnityEngine.SceneManagement; // 務必引入此命名空間

public class EnemySpawner : MonoBehaviour
{
    #region 基礎原件
    [SerializeField]
    private GameObject _enemyPrefab; // 敵人預製物

    #endregion
    void Start()
    {
        Spawn();
    }

    void Spawn()
    {
       // 1.生成敵人物件並取得 GameObject 實體
        GameObject spawnedEnemy = Instantiate(_enemyPrefab, transform.position, transform.rotation);

        // 2. 強制將生成的敵人物件搬移到 EnemySpawner 所在的關卡場景中 (例如 Stage01 / Stage02)
        SceneManager.MoveGameObjectToScene(spawnedEnemy, gameObject.scene);
    }
}
