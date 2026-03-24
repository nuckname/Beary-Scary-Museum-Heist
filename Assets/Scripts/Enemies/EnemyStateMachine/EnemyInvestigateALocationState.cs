using UnityEngine;

public class EnemyInvestigateALocationState : EnemyBaseState
{
    private bool reachedLocation = false;

    // Look-around variables
    private int targetLooks;
    private int looksCompleted;
    private Quaternion baseRotation;
    private Quaternion targetLookRotation;
    private bool lookingRight;

    public override void EnterState(EnemyStateManager manager)
    {
        Debug.Log("State: Investigating!");
        reachedLocation = false;

        UpdateGuardsText(manager);
    }

    // Can refactor this to EnemyStateManager onto another script if we want to avoid having to update the text in every state, but for now this is fine
    private void UpdateGuardsText(EnemyStateManager manager)
    {
        if (manager.stateText != null)
        {
            manager.stateText.text = "I SEE YOUUU";
            manager.stateText.color = Color.yellow;
        }
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        Vector3 targetLocation = manager.investigateTargetPosition;
        targetLocation.y = manager.transform.position.y; // Keep it on the same height plane

        if (!reachedLocation)
        {
            // Rotate and Move towards the suspicious location
            Vector3 direction = (targetLocation - manager.transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                manager.transform.rotation = Quaternion.RotateTowards(manager.transform.rotation, targetRotation, manager.turnSpeed * Time.deltaTime);
            }

            manager.transform.position = Vector3.MoveTowards(manager.transform.position, targetLocation, manager.walkSpeed * Time.deltaTime);

            // Did we arrive?
            if (Vector3.Distance(manager.transform.position, targetLocation) < 0.5f)
            {
                reachedLocation = true;

                // Look around behaviour
                baseRotation = manager.transform.rotation;
                targetLooks = Random.Range(3, 6); 
                looksCompleted = 0;
                
                // Randomly choose to look left or right first
                lookingRight = Random.value > 0.5f; 
                SetNextLookRotation();
                
                if (manager.stateText != null)
                {
                    manager.stateText.text = "Sus?";
                    manager.stateText.color = Color.yellow;
                }
            }
        }
        else
        {
            // Rotate towards the current left/right target
            manager.transform.rotation = Quaternion.RotateTowards(manager.transform.rotation, targetLookRotation, manager.turnSpeed * Time.deltaTime);

            // If the angle between current rotation and target rotation is very small, we've completed one "look"
            if (Quaternion.Angle(manager.transform.rotation, targetLookRotation) < 2f)
            {
                looksCompleted++;

                if (looksCompleted >= targetLooks)
                {
                    Debug.Log("Nothing here. Switching to Lost Player state.");
                    manager.SwitchState(manager.EnemyLostPlayerState); 
                }
                else
                {
                    // Switch direction for the next look
                    lookingRight = !lookingRight;
                    SetNextLookRotation();
                }
            }
        }
    }

    // Helper method to calculate the angle to look at
    private void SetNextLookRotation()
    {
        // Turn 60 degrees left or right from the original base rotation
        float angle;

        if (lookingRight)
        {
            angle = 60f;
        }
        else
        {
            angle = -60f;
        }
        
        targetLookRotation = baseRotation * Quaternion.Euler(0, angle, 0);
    }

    public override void OnCollisionEnter2D(EnemyStateManager manager, Collision2D other)
    {
        
    }
}