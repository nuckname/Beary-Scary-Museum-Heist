using UnityEngine;

//https://www.youtube.com/watch?v=Vt8aZDPzRjI
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

    // State Machine mechanics
    public EnemyBaseState currentState;
    public EnemyFollowPathState FollowPathState = new EnemyFollowPathState();
    public EnemyInvestigateALocationState InvestigateState = new EnemyInvestigateALocationState();

    void Start()
    {
        // 1. Extract waypoints from the pathHolder just like your old Guards.cs
        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < pathHolder.childCount; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;    
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        // 2. Start in the Follow Path state
        SwitchState(FollowPathState);
    }

    void Update()
    {
        currentState?.UpdateState(this);
    }

    public void SwitchState(EnemyBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }

    // Use this to change stats
    public void TriggerInvestigation(Vector3 targetLocation)
    {
        // Dot trigger if we are already investigating this exact spot, prevents spamming
        if (currentState == InvestigateState && investigateTargetPosition == targetLocation) return;

        investigateTargetPosition = targetLocation;
        SwitchState(InvestigateState);
    }
}