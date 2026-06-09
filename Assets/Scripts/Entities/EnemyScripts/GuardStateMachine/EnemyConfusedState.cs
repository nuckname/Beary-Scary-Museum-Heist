using Unity.VisualScripting.FullSerializer;
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

    public int lookAngle; // How far the guard turns left and right when looking around

    public override void EnterState(EnemyStateManager manager)
    {
        // Stop
        manager.GuardStopMoving();
        
        // Initialize look around behaviour based on where they are currently facing
        baseRotation = manager.transform.rotation;
        
        targetLooks = manager.amountOfTimesTheGuardTurns;
        
        looksCompleted = 0;

        lookingRight = manager.playerLeftTheGuardsFovOnRightSide;
        SetNextLookRotation();

        manager.animator.SetBool("isMoving", false);
        manager.animator.SetBool("isChasing", false);
        
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
                manager.GuardStartMoving();
                
                Debug.Log("Guard finished looking around and didn't find anything, going back to patrolling.");
                
                manager.animator.SetBool("isMoving", true);
                
                manager.SetStateIcon(EnemyStateIcon.FinishedLookingAroundAndDidntFindAnythingSoBackToPatrolling);

                manager.turnTargetPosition = manager.waypoints[manager.currentWaypointIndex];
                manager.stateToSwitchToAfterTurning = manager.EnemyFollowPathState;
                manager.SwitchState(manager.EnemyTurnToPointState); 
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
        // Turn a set amount of degrees (lookAngle) left or right from the original base rotation
        float angle = lookingRight ? lookAngle : -lookAngle;
        targetLookRotation = baseRotation * Quaternion.Euler(0, angle, 0);
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
        // Collision logic
    }
}