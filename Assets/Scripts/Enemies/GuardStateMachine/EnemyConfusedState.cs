using UnityEngine;


/// <summary>
///  Enemy will turn left and right a few times looking for the player, then return to patrolling if they don't find them
/// </summary>
public class EnemyConfusedState : EnemyBaseState
{
    private int targetLooks;
    private int looksCompleted;
    private Quaternion baseRotation;
    private Quaternion targetLookRotation;
    private bool lookingRight;

    public override void EnterState(EnemyStateManager manager)
    {
        manager.currentWalkSpeed = manager.normalWalkSpeed;

        // Initialize look around behaviour based on where they are currently facing
        baseRotation = manager.transform.rotation;
        targetLooks = Random.Range(manager.minAmountOfTurnsTheGuardDoes, manager.maxAmountOfTurnsTheGuardDoes); 
        looksCompleted = 0;
        
        // Randomly choose to look left or right first
        lookingRight = Random.value > 0.5f; 
        SetNextLookRotation();
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        // Rotate towards the current left/right target
        manager.transform.rotation = Quaternion.RotateTowards(manager.transform.rotation, targetLookRotation, manager.turnSpeed * Time.deltaTime);

        // If the angle between current rotation and target rotation is very small, we've completed one "look"
        if (Quaternion.Angle(manager.transform.rotation, targetLookRotation) < 2f)
        {
            looksCompleted++;

            if (looksCompleted >= targetLooks)
            {
                manager.SwitchState(manager.EnemyFollowPathState); 
            }
            else
            {
                // Switch direction for the next look
                lookingRight = !lookingRight;
                SetNextLookRotation();
            }
        }
    }

    private void SetNextLookRotation()
    {
        // Turn 60 degrees left or right from the original base rotation
        float angle = lookingRight ? 60f : -60f;
        targetLookRotation = baseRotation * Quaternion.Euler(0, angle, 0);
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
        // Collision logic
    }
}