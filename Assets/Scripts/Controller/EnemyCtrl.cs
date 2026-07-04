using UnityEngine;

public class EnemyCtrl : BaseCtrl
{
    #region AI參數
    private Transform _target;
    private Vector2 _aiMoveInput;
    [SerializeField]
    private float _patrolRange = 3f;
    private bool _patrolling;
    private Vector3 _patrolPos;
    [SerializeField]
    private float _chaseRange = 10f;
    [SerializeField]
    private float _attackRange = 2f;
    #endregion AI參數

    /// <summary>
    /// 目標對象(玩家)
    /// </summary>
    public Transform Target => _target ??= GameManager.playerCtrl.transform;

    public override Vector2 MoveInput => _aiMoveInput;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
