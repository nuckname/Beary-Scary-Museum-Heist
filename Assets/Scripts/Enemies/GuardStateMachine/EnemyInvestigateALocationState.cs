using UnityEngine;

public class EnemyInvestigateALocationState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager manager)
    {
        manager.SetStateIcon(EnemyStateIcon.Confused);
        
        manager.GuardStopMoving();
        manager.agent.speed = manager.guardPatrollSpeed;
        
        manager.agent.SetDestination(manager.investigateTargetPosition);
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        if (!manager.agent.pathPending && manager.agent.remainingDistance < 0.5f)
        {
            manager.SwitchState(manager.enemyConfusedState);
        }
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
        // Collision logic
    }
}