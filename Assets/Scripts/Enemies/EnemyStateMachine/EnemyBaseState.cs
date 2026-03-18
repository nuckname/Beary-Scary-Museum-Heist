using UnityEngine;

public abstract class EnemyBaseState
{
    public abstract void EnterState(EnemyStateManager enemyStateManager);
    public abstract void UpdateState(EnemyStateManager enemyStateManager);
    public abstract void OnCollisionEnter2D(EnemyStateManager enemyStateManager, Collision2D other);
}