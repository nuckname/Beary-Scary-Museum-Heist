using UnityEngine;

/// <summary>
/// Enemy will be stunned for a short duration, unable to see or move, before recovering and acting confused for a moment
/// </summary>
public class EnemyStunnedState : EnemyBaseState
{
    private float stunTimer = 0f;
    private float stunDuration = 1.5f; 

    public override void EnterState(EnemyStateManager manager)
    {
        stunTimer = 0f;

        manager.agent.isStopped = true;
        manager.agent.velocity = Vector3.zero;
        
        manager.animator.SetBool("isStunned", true);
        
        
        manager.SetStateIcon(EnemyStateIcon.GuardIsStunned);
        
        if (manager.fieldOfViews != null)
        {
            foreach (var fov in manager.fieldOfViews)
            {
                if (fov != null) fov.DisableVision();
            }
        }
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        stunTimer += Time.deltaTime;

        if (stunTimer >= stunDuration)
        {
            // Recovering from stun
            if (manager.fieldOfViews != null)
            {
                foreach (var fov in manager.fieldOfViews)
                {
                    if (fov != null) fov.RestoreVision();
                }
            }
          
            manager.animator.SetBool("isStunned", false);
            
            // Just act confused?
            manager.SwitchState(manager.enemyConfusedState);
        }
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
    }
}