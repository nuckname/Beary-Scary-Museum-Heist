using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// The central state machine and facade for the enemy AI. 
/// </summary>
public class EnemyStateManager : MonoBehaviour
{
    public EnemyLocomotion locomotion;
    public EnemySensors sensors;
    public EnemyVisuals visuals;
    
    // --- Facade Properties --
    public NavMeshAgent agent => locomotion.agent;
    public Animator animator => visuals.animator;
    public Vector3[] waypoints => locomotion.waypoints;
    public float turnSpeed => locomotion.turnSpeed;
    public float waitTime => locomotion.waitTime;
    public float guardPatrollSpeed => locomotion.guardPatrollSpeed;
    public float guardChaseSpeed => locomotion.guardChaseSpeed;
    public FieldOfView[] fieldOfViews => sensors.fieldOfViews;
    public float fovTurnAngleThreshold => sensors.fovTurnAngleThreshold;
    public float turnVisionRadiusReductionPercentage => sensors.turnVisionRadiusReductionPercentage;
    public bool alwaysTurnRight { get => locomotion.alwaysTurnRight; set => locomotion.alwaysTurnRight = value; }
    public bool defaultAlwaysTurnRight => locomotion.defaultAlwaysTurnRight;

    [Header("Investigation")]
    [HideInInspector] public Vector3 investigateTargetPosition;

    [Header("Behaviour Settings")]
    [Tooltip("If true, when the guards lose sight of the player, they will go to the player's last known location and look around. If false, they will instantly look around and then patrol.")]
    [InspectorName("Guards Go To Last Player Location When They Lose Sight")]
    public bool makeGuardsInvestiageLastPlayerLocationWhenTheyLoseSight = true;

    [Header("Guard Turning")]
    [Tooltip("The amount of turns the guard does when they lose sight of the player and look around. " +
             "A turn is either looking left or right. So if this is 2, they will look left, then right, then go back to patrolling. " +
             "If this is 4, they will look left, right, left, right before going back to patrolling.")]
    public int amountOfTimesTheGuardTurns = 2;
    public int turnAngle = 40;

    // State Instances
    [HideInInspector] public EnemyBaseState EnemyCurrentState; 
    [HideInInspector] public EnemyFollowPathState EnemyFollowPathState = new EnemyFollowPathState();
    [HideInInspector] public EnemyInvestigateALocationState EnemyInvestigateState = new EnemyInvestigateALocationState();
    [HideInInspector] public EnemyConfusedState enemyConfusedState = new EnemyConfusedState();
    [HideInInspector] public EnemyStunnedState EnemyStunnedState = new EnemyStunnedState();
    [HideInInspector] public EnemyChasePlayerState EnemyChasePlayerState = new EnemyChasePlayerState();
    
    // Before we use EnemeyTurnState, we must allow tell it what state to go to afterwards using stateToSwitchToAfterTurning
    [HideInInspector] public EnemyTurnToPointState EnemyTurnToPointState = new EnemyTurnToPointState();
    [HideInInspector] public Vector3 turnTargetPosition;
    public EnemyBaseState stateToSwitchToAfterTurning;
    
    // This is currently keeping track of what waypoint to turn towards when are we are a state.
    [HideInInspector] public int currentWaypointIndex = 0;

    public bool playerLeftTheGuardsFovOnRightSide = false;
    
    // Can make this an enum later
    [Header("Debug")]
    public string currentStateName;
    public bool isTutorial = false;

    private void Awake()
    {
        locomotion = GetComponent<EnemyLocomotion>();
        sensors = GetComponent<EnemySensors>();
        visuals = GetComponent<EnemyVisuals>();
    }

    void Start()
    {
        enemyConfusedState.lookAngle = turnAngle;
    }

    void Update()
    {
        EnemyCurrentState?.UpdateState(this);
    }

    // Use this to switch states
    public void SwitchState(EnemyBaseState state)
    {
        Debug.Log($"Switching State TO: {state.GetType().Name}");
        
        EnemyCurrentState = state;
        
        sensors.RestoreAllFOVRadiuses();

        // Always turn false but then override this for something in a specific state. 
        locomotion.alwaysTurnRight = false;
        
        if (EnemyCurrentState != null)
        {
            currentStateName = EnemyCurrentState.GetType().Name;
            EnemyCurrentState.EnterState(this);
        }
    }

    public void StartChasing(Transform target)
    {
        sensors.playerTransform = target;

        // Only switch state if we aren't ALREADY chasing them and NOT stunned
        if (EnemyCurrentState != EnemyChasePlayerState && EnemyCurrentState != EnemyStunnedState)
        {
            locomotion.ResetChaseSpeed();
            SwitchState(EnemyChasePlayerState);
        }
    }

    // Facade Methods for Locomotion/Visuals to keep States working
    public void GuardStartMoving() => locomotion.GuardStartMoving();
    public void InitialiseGuardStartMoving() => locomotion.InitialiseGuardStartMoving();
    public void GuardStopMoving() => locomotion.GuardStopMoving();
    public void SetStateIcon(EnemyStateIcon iconType) => visuals.SetStateIcon(iconType);

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            SwitchState(EnemyStunnedState);
            return;
        }
        
        EnemyCurrentState?.OnCollisionEnter(this, collision);
    }
}