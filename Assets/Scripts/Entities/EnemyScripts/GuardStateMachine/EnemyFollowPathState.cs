using UnityEngine;

/// <summary>
/// Handles the default patrol behavior, guiding the enemy along a sequence of NavMesh waypoints. 
/// Manages wait timers at each node, precise rotational alignments before moving to the next waypoint, 
/// and dynamically reduces the Field of View (FOV) radius during sharp turns to simulate realistic vision constraints.
/// </summary>
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
        manager.agent.speed = manager.guardPatrollSpeed;

        manager.GuardStartMoving();
        manager.animator.SetBool("isMoving", true);
        
        if (manager.waypoints.Length > 0)
        {
            CheckWhichWaypointToGoTo(manager);
            manager.agent.SetDestination(manager.waypoints[manager.currentWaypointIndex]);
        }
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        if (manager.waypoints.Length == 0) return;

        Vector3 targetWaypoint = manager.waypoints[manager.currentWaypointIndex];

        // Adjust Vision Radius based on turn severity
        HandleVisionReduction(manager, targetWaypoint);

        // Process movement or waiting behavior
        if (isWaiting)
        {
            ProcessWaitingLogic(manager, targetWaypoint);
        }
        else
        {
            CheckForArrival(manager);
        }
    }

    private void HandleVisionReduction(EnemyStateManager manager, Vector3 targetWaypoint)
    {
        Vector3 desiredDirection = GetDesiredDirection(manager, targetWaypoint);
        
        if (desiredDirection == Vector3.zero)
        {
            ApplyFOVState(manager, false);
            return;
        }

        float angleToTarget = Vector3.Angle(manager.transform.forward, desiredDirection);
        bool isTurningSharply = angleToTarget >= manager.fovTurnAngleThreshold;
        
        ApplyFOVState(manager, isTurningSharply);
    }

    private Vector3 GetDesiredDirection(EnemyStateManager manager, Vector3 targetWaypoint)
    {
        Vector3 desiredDirection = Vector3.zero;

        if (isWaiting)
        {
            desiredDirection = (targetWaypoint - manager.transform.position).normalized;
            manager.animator.SetBool("isMoving", false);
        }
        else if (manager.agent.hasPath)
        {
            desiredDirection = (manager.agent.steeringTarget - manager.transform.position).normalized;
        }

        desiredDirection.y = 0; // Flatten axis
        return desiredDirection;
    }

    private void ApplyFOVState(EnemyStateManager manager, bool reduceRadius)
    {
        if (manager.fieldOfViews == null) return;

        foreach (var fov in manager.fieldOfViews)
        {
            if (fov == null) continue;

            if (reduceRadius)
                fov.ReduceFOVRadius(manager.turnVisionRadiusReductionPercentage);
            else
                fov.RestoreFOVRadius();
        }
    }

    private void ProcessWaitingLogic(EnemyStateManager manager, Vector3 targetWaypoint)
    {
        waitTimer += Time.deltaTime;
        
        RotateTowards(manager, targetWaypoint);

        Vector3 directionToTarget = (targetWaypoint - manager.transform.position).normalized;
        directionToTarget.y = 0; 
        
        // Check if the angle is less than 1f degree to account for tiny math inaccuracies 
        bool hasFinishedTurning = Vector3.Angle(manager.transform.forward, directionToTarget) < 1f;

        if (waitTimer >= manager.waitTime && hasFinishedTurning)
        {
            ResumePatrol(manager, targetWaypoint);
        }
    }

    private void ResumePatrol(EnemyStateManager manager, Vector3 targetWaypoint)
    {
        isWaiting = false;
        waitTimer = 0f;
        
        manager.animator.SetBool("isMoving", true);
        manager.agent.isStopped = false;
        manager.agent.SetDestination(targetWaypoint); 
    }

    private void CheckForArrival(EnemyStateManager manager)
    {
        if (!manager.agent.pathPending && manager.agent.remainingDistance <= manager.agent.stoppingDistance + 0.1f)
        {
            manager.currentWaypointIndex = (manager.currentWaypointIndex + 1) % manager.waypoints.Length;
            
            manager.animator.SetBool("isMoving", false);
            manager.SetStateIcon(EnemyStateIcon.HideIcon);
            
            isWaiting = true; 
            manager.agent.isStopped = true; 
        }
    }

    private void CheckWhichWaypointToGoTo(EnemyStateManager manager)
    {
        int currentIndex = manager.currentWaypointIndex;
        int nextIndex = (currentIndex + 1) % manager.waypoints.Length;

        float distanceToCurrent = Vector3.Distance(manager.transform.position, manager.waypoints[currentIndex]);
        float distanceToNext = Vector3.Distance(manager.transform.position, manager.waypoints[nextIndex]);

        if (distanceToCurrent <= manager.agent.stoppingDistance + 5f)
        {
            manager.currentWaypointIndex = nextIndex;
            return;
        }

        if (distanceToNext < distanceToCurrent)
        {
            manager.currentWaypointIndex = nextIndex;
        }
    }

    // AI
    public void RotateTowards(EnemyStateManager manager, Vector3 targetPos)
    {
        Vector3 direction = (targetPos - manager.transform.position).normalized;
        direction.y = 0; 

        if (direction == Vector3.zero) return;

        if (manager.alwaysTurnRight)
        {
            // 1. Get the signed angle (-180 to 180 degrees) from current forward to the target
            float signedAngle = Vector3.SignedAngle(manager.transform.forward, direction, Vector3.up);
            
            // 2. Convert to a 0 to 360 degree range
            float clockwiseAngle = signedAngle >= 0f ? signedAngle : 360f + signedAngle;
            float rotationStep = manager.turnSpeed * Time.deltaTime;

            // 3. Snap to target if close enough
            if (clockwiseAngle <= rotationStep || clockwiseAngle >= 359f)
            {
                manager.transform.rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                manager.transform.Rotate(Vector3.up, rotationStep, Space.World);
            }
        }
        else
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            manager.transform.rotation = Quaternion.RotateTowards(manager.transform.rotation, targetRotation, manager.turnSpeed * Time.deltaTime);

            if (manager.defaultAlwaysTurnRight)
            {
                manager.alwaysTurnRight = true;
            }
        }
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
    }
}