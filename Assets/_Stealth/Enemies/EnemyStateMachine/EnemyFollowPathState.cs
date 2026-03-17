using UnityEngine;

// A state for walking around just follows transforms
public class EnemyFollowPathState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager roundStateManager)
    {
    }

    public override void UpdateState(EnemyStateManager roundStateManager)
    {
        
    }

    public override void OnCollisionEnter2D(EnemyStateManager roundStateManager, Collision2D other)
    {
        //Hand shakes
    }
}