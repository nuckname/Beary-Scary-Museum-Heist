using UnityEngine;

public class EnemyStateManager : MonoBehaviour
{
    private EnemyBaseState currentState;

    public EnemyFollowPathState FollowPathState = new EnemyFollowPathState();
    public EnemyInvestigateALocationState InvestigateState = new EnemyInvestigateALocationState();

    void Start()
    {
        // Starting state
        currentState = FollowPathState;
        currentState.EnterState(this);
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        currentState?.OnCollisionEnter2D(this, other);
    }
}