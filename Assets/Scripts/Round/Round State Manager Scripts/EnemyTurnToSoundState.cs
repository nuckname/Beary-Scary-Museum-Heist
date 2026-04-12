using UnityEngine;

public class EnemyTurnToSoundState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager manager)
    {
        manager.SetStateIcon(EnemyStateIcon.HeardASound);
        
        manager.GuardStopMoving();
    }

    // AI
    public override void UpdateState(EnemyStateManager manager)
    {
        // 1. Calculate the direction to the sound
        Vector3 directionToSound = (manager.investigateTargetPosition - manager.transform.position).normalized;
        
        // Flatten the Y axis so the guard doesn't tilt up into the sky or down into the floor
        directionToSound.y = 0;

        // Failsafe: if the sound is exactly where we are standing, skip turning
        if (directionToSound == Vector3.zero)
        {
            manager.SwitchState(manager.EnemyInvestigateState);
            return;
        }

        // 2. Find the rotation we need to look at that direction
        Quaternion targetRotation = Quaternion.LookRotation(directionToSound);

        // 3. Smoothly rotate the guard towards that direction using the turnSpeed variable
        manager.transform.rotation = Quaternion.RotateTowards(
            manager.transform.rotation, 
            targetRotation, 
            manager.turnSpeed * Time.deltaTime
        );

        // 4. Check if we are facing the sound (within a tiny margin of error, like 2 degrees)
        if (Quaternion.Angle(manager.transform.rotation, targetRotation) < 2f)
        {
            // We finished turning! Now trigger the walk state
            manager.SwitchState(manager.EnemyInvestigateState);
        }
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
    }
}