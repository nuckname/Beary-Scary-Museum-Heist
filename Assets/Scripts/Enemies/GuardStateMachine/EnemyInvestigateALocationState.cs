using UnityEngine;

public class EnemyInvestigateALocationState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager manager)
    {
        manager.SetStateIcon(EnemyStateIcon.LookingAroundConfused);
        
        manager.agent.speed = manager.guardChaseSpeed;
        
        
        manager.GuardStartMoving(); 
        
        manager.animator.SetBool("IsAlerted", true);
        manager.animator.SetBool("IsWalking", true);
        
        manager.agent.SetDestination(manager.investigateTargetPosition);
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        if (manager.agent.pathPending) return;

        if (manager.agent.remainingDistance <= 1f)
        {
            manager.SwitchState(manager.enemyConfusedState);
        }
        // Fallback, If the path becomes invalid or unreachable, don't get stuck forever
        else if (manager.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial || 
                 manager.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            manager.SwitchState(manager.enemyConfusedState);
        }
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
        // Collision logic
    }
}