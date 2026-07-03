using UnityEngine;

public class SkillCtrl : MonoBehaviour
{

    /// <summary>
    /// [列舉]目標類型
    /// </summary>
    public enum Target { None, Enemy, Player }
    [SerializeField]
    private Target target;
    /// <summary>
    /// 撞擊效果
    /// </summary>
    [SerializeField]
    private GameObject _hitEffectObj;
    /// <summary>
    /// 銷毀時間
    /// </summary>
    [SerializeField]
    private float _destroyTime = 2f;

    /// <summary>
    /// 實際運作的對象目標標籤
    /// </summary>
    private string Tag
    {
        get
        {
            switch (target)
            {
                case Target.Enemy: return "Enemy";
                case Target.Player: return "Player";
            }
            return string.Empty;
        }
    }

    void Start()
    {
        Destroy(gameObject, _destroyTime);
    }

    /// <summary>
    /// 物件上必須要有碰撞器，且勾上IsTrigger
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tag))
        {     
            _hitEffectObj.SetActive(true);
            
        }
    }
}
