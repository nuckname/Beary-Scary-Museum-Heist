using UnityEngine;

public abstract class EnemyBaseState
{
    public abstract void EnterState(EnemyStateManager enemyStateManager);
    public abstract void UpdateState(EnemyStateManager enemyStateManager);

    public virtual void OnCollisionEnter(EnemyStateManager enemyStateManager, Collision other)
    {
        
    }
}