using UnityEngine;

public class EnemyInvestigateALocationState : EnemyBaseState
{
    private float searchTimer = 0f;
    private float searchDuration = 3f; 
    private bool reachedLocation = false;

    public override void EnterState(EnemyStateManager manager)
    {
        Debug.Log("State: Investigating!");
        searchTimer = 0f;
        reachedLocation = false;
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
                //Do like random rotation
                Debug.Log("Arrived at suspicious location. Looking around...");
            }
        }
        else
        {
            // Wait and look around
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchDuration)
            {
                Debug.Log("Nothing here. Returning to patrol.");
                manager.SwitchState(manager.EnemyFollowPathState); 
            }
        }
    }

    public override void OnCollisionEnter2D(EnemyStateManager manager, Collision2D other)
    {
        
    }
}