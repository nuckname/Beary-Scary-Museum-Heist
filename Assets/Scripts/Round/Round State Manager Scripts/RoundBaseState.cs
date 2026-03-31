using UnityEngine;

public abstract class RoundBaseState
{
    public abstract void EnterState(RoundStateManager manager);
    public abstract void UpdateState(RoundStateManager manager);
    public abstract void OnCollisionEnter(RoundStateManager manager, Collision collision);
    public abstract void OnCollisionExit(RoundStateManager manager, Collision collision);
    public abstract void OnTriggerEnter(RoundStateManager manager, Collider other);
    public abstract void OnTriggerExit(RoundStateManager manager, Collider other);
}