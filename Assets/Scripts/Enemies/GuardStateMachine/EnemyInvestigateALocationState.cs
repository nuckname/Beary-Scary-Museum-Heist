using UnityEngine;

public class EnemyInvestigateALocationState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager manager)
    {
        manager.SetStateIcon(EnemyStateIcon.LookingAroundConfused);
        
        manager.agent.speed = manager.guardPatrollSpeed;
        manager.GuardStartMoving(); 
        
        manager.agent.SetDestination(manager.investigateTargetPosition);
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        if (manager.agent.remainingDistance < 0.05f)
        {
            manager.SwitchState(manager.enemyConfusedState);
        }
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
        // Collision logic
    }
}