using UnityEngine;

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
        Instantiate(_enemyPrefab, transform.position, transform.rotation);
    }
}
