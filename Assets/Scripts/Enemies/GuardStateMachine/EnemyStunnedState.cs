using UnityEngine;

/// <summary>
/// Enemy will be stunned for a short duration, unable to see or move, before recovering and acting confused for a moment
/// </summary>
public class EnemyStunnedState : EnemyBaseState
{
    private float stunTimer = 0f;
    private float stunDuration = 0.75f; 

    public override void EnterState(EnemyStateManager manager)
    {
        stunTimer = 0f;

       // manager.SetStateText("STUNNED", Color.gray);

        if (manager.fieldOfView != null)
        {
            manager.fieldOfView.DisableVision();
        }
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        stunTimer += Time.deltaTime;

        if (stunTimer >= stunDuration)
        {
            // Recovering from stun
            if (manager.fieldOfView != null)
            {
                manager.fieldOfView.RestoreVision();
            }

            // Just act confused?
            manager.SwitchState(manager.EnemyLostPlayerState);
        }
    }

    public override void OnCollisionEnter2D(EnemyStateManager manager, Collision2D other)
    {
    }
}