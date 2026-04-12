using UnityEngine;

public class EnemyFollowPathState : EnemyBaseState
{
    private float waitTimer = 0f;
    private bool isWaiting = false;

    public override void EnterState(EnemyStateManager manager)
    {
        isWaiting = false;
        waitTimer = 0f;
        manager.SetStateIcon(EnemyStateIcon.HideIcon);

        manager.agent.angularSpeed = manager.turnSpeed;

        manager.GuardStartMoving();
        manager.agent.speed = manager.guardPatrollSpeed;

        // Start heading to the first target using the manager's index
        if (manager.waypoints.Length > 0)
        {
            manager.agent.SetDestination(manager.waypoints[manager.currentWaypointIndex]);
        }
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        if (manager.waypoints.Length == 0) return;

        Vector3 targetWaypoint = manager.waypoints[manager.currentWaypointIndex];

        Vector3 desiredDirection = Vector3.zero;

        if (isWaiting)
        {
            // When waiting, the guard is manually rotating towards the next waypoint
            desiredDirection = (targetWaypoint - manager.transform.position).normalized;
        }
        else if (manager.agent.hasPath)
        {
            // When walking, the steeringTarget is the immediate next corner in the NavMesh path
            desiredDirection = (manager.agent.steeringTarget - manager.transform.position).normalized;
        }

        desiredDirection.y = 0; // Flatten

        // Check the angle for FOV reduction
        if (desiredDirection != Vector3.zero)
        {
            float angleToTarget = Vector3.Angle(manager.transform.forward, desiredDirection);
            
            if (angleToTarget >= manager.fovTurnAngleThreshold)
            {
                // The angle variation is 25+ degrees, reduce vision radius
                if (manager.fieldOfViews != null)
                {
                    foreach (var fov in manager.fieldOfViews)
                    {
                        if (fov != null) fov.ReduceFOVRadius(manager.turnVisionRadiusReductionPercentage);
                    }
                }
            }
            else
            {
                // We are facing mostly forward, return radius to normal
                if (manager.fieldOfViews != null)
                {
                    foreach (var fov in manager.fieldOfViews)
                    {
                        if (fov != null) fov.RestoreFOVRadius();
                    }
                }
            }
        }
        else
        {
            if (manager.fieldOfViews != null)
            {
                foreach (var fov in manager.fieldOfViews)
                {
                    if (fov != null) fov.RestoreFOVRadius();
                }
            }
        }

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            
            // Turn to face the next waypoint while waiting
            RotateTowards(manager, targetWaypoint);

            // Check if the guard has fully rotated to face the next waypoint
            Vector3 directionToTarget = (targetWaypoint - manager.transform.position).normalized;
            directionToTarget.y = 0; 
            
            // We check if the angle is less than 1f degree to account for tiny math inaccuracies 
            bool hasFinishedTurning = Vector3.Angle(manager.transform.forward, directionToTarget) < 1f;

            // Only stop waiting if the timer is up AND we have fully finished turning
            if (waitTimer >= manager.waitTime && hasFinishedTurning)
            {
                isWaiting = false;
                waitTimer = 0f;
                
                // Wait and turn is over, tell the agent to resume moving
                manager.agent.isStopped = false;
                manager.agent.SetDestination(targetWaypoint); 
            }
            
            return; 
        }

        // Did we arrive? 
        if (!manager.agent.pathPending && manager.agent.remainingDistance <= manager.agent.stoppingDistance + 0.1f)
        {
            manager.currentWaypointIndex = (manager.currentWaypointIndex + 1) % manager.waypoints.Length;
            
            isWaiting = true; 
            manager.agent.isStopped = true; // Stop the agent from walking while waiting

            manager.SetStateIcon(EnemyStateIcon.HideIcon);
        }
    }
    
    // AI
    // https://gemini.google.com/share/93d7d274a35c
    public void RotateTowards(EnemyStateManager manager, Vector3 targetPos)
    {
        Vector3 direction = (targetPos - manager.transform.position).normalized;
        direction.y = 0; // Flatten the Y axis so the guard doesn't tilt up/down

        if (direction != Vector3.zero)
        {
            if (manager.alwaysTurnRight)
            {
                // 1. Get the signed angle (-180 to 180 degrees) from current forward to the target
                float signedAngle = Vector3.SignedAngle(manager.transform.forward, direction, Vector3.up);
                
                // 2. Convert to a 0 to 360 degree range to measure the exact clockwise distance
                float clockwiseAngle = signedAngle >= 0f ? signedAngle : 360f + signedAngle;
                
                float rotationStep = manager.turnSpeed * Time.deltaTime;

                // 3. Check if we are close enough to snap to the exact rotation 
                // (Using a small threshold near 360f prevents a full spin if the agent starts slightly left)
                if (clockwiseAngle <= rotationStep || clockwiseAngle >= 359f)
                {
                    manager.transform.rotation = Quaternion.LookRotation(direction);
                }
                else
                {
                    // 4. Always rotate right (positive Y axis rotation)
                    manager.transform.Rotate(Vector3.up, rotationStep, Space.World);
                }
            }
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                manager.transform.rotation = Quaternion.RotateTowards(manager.transform.rotation, targetRotation, manager.turnSpeed * Time.deltaTime);
            }
        }
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
    }
}