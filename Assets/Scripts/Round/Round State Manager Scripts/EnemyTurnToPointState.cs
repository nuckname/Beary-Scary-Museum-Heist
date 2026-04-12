using UnityEngine;

public class EnemyTurnToPointState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager manager)
    {
        manager.GuardStopMoving();
    }

    // AI
    public override void UpdateState(EnemyStateManager manager)
    {
        // 1. Use the new generic target position variable
        Vector3 directionToPoint = (manager.turnTargetPosition - manager.transform.position).normalized;
        directionToPoint.y = 0;

        // Failsafe
        if (directionToPoint == Vector3.zero)
        {
            manager.SwitchState(manager.stateToSwitchToAfterTurning);
            return;
        }

        // 2. Find the rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToPoint);

        // 3. Smoothly rotate
        manager.transform.rotation = Quaternion.RotateTowards(
            manager.transform.rotation, 
            targetRotation, 
            manager.turnSpeed * Time.deltaTime
        );

        // 4. Check if we are facing the point
        if (Quaternion.Angle(manager.transform.rotation, targetRotation) < 2f)
        {
            // Go to whatever state the manager told us to go to!
            manager.SwitchState(manager.stateToSwitchToAfterTurning);
        }
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
        // Collision logic
    }
}