using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages the physical movement and pathfinding of the enemy. 
/// Handles NavMeshAgent speed adjustments, patrol route setup, and dynamic chase acceleration mechanics.
/// </summary>
public class EnemyLocomotion : MonoBehaviour
{
    private EnemyStateManager manager;
    private EnemyVisuals visuals;

    [Header("Pathfinding")]
    public Transform pathHolder;
    public float turnSpeed = 90f;
    public float waitTime = 2f;
    [HideInInspector] public Vector3[] waypoints;

    [Header("Path Visuals")] 
    public float guardPatrollSpeed = 3f;
    public float guardChaseSpeed = 3f;
    
    [Header("Chase Escalation Settings")]
    public float maxChaseSpeed = 7f;
    public float chaseAcceleration = 1.5f;
    private float currentChaseSpeed;

    [Header("Movement Settings")]
    [Tooltip("If true, the guard will only rotate clockwise. If false, they will take the shortest rotation path.")]
    public bool alwaysTurnRight = false;
    
    // Add this to remember the guard's specific inspector setting
    [HideInInspector] public bool defaultAlwaysTurnRight;
    
    [HideInInspector] public NavMeshAgent agent;

    private void Awake()
    {
        manager = GetComponent<EnemyStateManager>();
        visuals = GetComponent<EnemyVisuals>();
        agent = GetComponent<NavMeshAgent>();
        defaultAlwaysTurnRight = alwaysTurnRight;
    }

    private void Start()
    {
        if (pathHolder == null)
        {
            Debug.LogError("Path Holder is not assigned on " + gameObject.name);
            return;
        }

        SetupPatrolRoute();

        // Freeze guards until we want them to start moving called in RoundStateManager
        agent.isStopped = true;
        agent.speed = 0;
    }

    private void Update()
    {
        IncreaseGuardSpeedOverTime();
    }

    private void SetupPatrolRoute()
    {
        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < pathHolder.childCount; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;    
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }
    }

    private void IncreaseGuardSpeedOverTime()
    {
        if (manager.sensors.isCurrentlySeeingPlayer && manager.EnemyCurrentState == manager.EnemyChasePlayerState)
        {
            if (currentChaseSpeed < maxChaseSpeed)
            {
                currentChaseSpeed += chaseAcceleration * Time.deltaTime;
                agent.speed = Mathf.Min(currentChaseSpeed, maxChaseSpeed);
                //Debug.Log("Enemy Speed: " + agent.speed);
            }
        }
    }

    public void ResetChaseSpeed()
    {
        currentChaseSpeed = guardChaseSpeed;
        agent.speed = currentChaseSpeed;
    }

    public void InitialiseGuardStartMoving()
    {
        agent.speed = guardPatrollSpeed;
        agent.isStopped = false;
        visuals.animator.SetBool("isMoving", true);
    }

    public void GuardStopMoving()
    {
        agent.isStopped = true;
        visuals.animator.SetBool("isMoving", false);
    }
    
    public void GuardStartMoving()
    {
        agent.isStopped = false;
        visuals.animator.SetBool("isMoving", true);
    }
}