using UnityEngine;

public class RoundStateManager : MonoBehaviour
{
    // State Instances
    [HideInInspector] public RoundBaseState CurrentState; 
    [HideInInspector] public RoundAboutToStartState AboutToStartState = new RoundAboutToStartState();
    [HideInInspector] public RoundInProgressState InProgressState = new RoundInProgressState();

    [Header("Debug")]
    public string currentStateName;

    void Start()
    {
        // Set the initial state when the scene starts
        SwitchState(AboutToStartState);
    }
    
    void Update()
    {
        // Call the update method of whatever state is currently active
        CurrentState?.UpdateState(this);
    }

    // Use this to switch states
    public void SwitchState(RoundBaseState state)
    {
        CurrentState = state;
        
        if (CurrentState != null)
        {
            currentStateName = CurrentState.GetType().Name;
            CurrentState.EnterState(this);
        }
    }
}