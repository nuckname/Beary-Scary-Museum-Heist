using TMPro;
using UnityEngine;

public class EnemyStateManager : MonoBehaviour
{
    [Header("Pathfinding")]
    public Transform pathHolder;
    public float walkSpeed = 2f;
    public float turnSpeed = 90f;
    public float waitTime = 2f;
    [HideInInspector] public Vector3[] waypoints;

    [Header("Investigation")]
    [HideInInspector] public Vector3 investigateTargetPosition;


    public EnemyBaseState EnemyCurrentState;
    public EnemyFollowPathState EnemyFollowPathState;
    public EnemyInvestigateALocationState EnemyInvestigateState;

    public TMP_Text stateText;
    
    // Can make this an enum later
    [Header("Debug")]
    public string currentStateName;
    
    void Awake()
    {
        EnemyFollowPathState = new EnemyFollowPathState();
        EnemyInvestigateState = new EnemyInvestigateALocationState();
    }

    void Start()
    {
        if (pathHolder == null)
        {
            Debug.LogError("Path Holder is not assigned on " + gameObject.name);
            return;
        }

        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < pathHolder.childCount; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;    
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        SwitchState(EnemyFollowPathState);
    }

    void Update()
    {
        EnemyCurrentState?.UpdateState(this);
    }

    // Use this to switch states
    public void SwitchState(EnemyBaseState state)
    {
        EnemyCurrentState = state;
        
        if (EnemyCurrentState != null)
        {
            currentStateName = EnemyCurrentState.GetType().Name;
            EnemyCurrentState.EnterState(this);
        }
    }

    public void TriggerInvestigation(Vector3 targetLocation)
    {
        // Dont trigger if we are already investigating this exact spot, prevents spamming
        if (EnemyCurrentState == EnemyInvestigateState && investigateTargetPosition == targetLocation) return;

        investigateTargetPosition = targetLocation;
        SwitchState(EnemyInvestigateState);
    }
}