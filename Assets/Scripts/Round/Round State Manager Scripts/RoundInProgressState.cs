using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class RoundInProgressState : RoundBaseState
{
    public override void EnterState(RoundStateManager manager)
    {
        Debug.Log("Round is in progress.");
        manager.timer.isPaused = false;
        manager.MakeGuardsStartMoving();
    }

    public override void UpdateState(RoundStateManager manager) { }

    public override void OnCollisionEnter(RoundStateManager manager, Collision collision)
    {
    }
    public override void OnCollisionExit(RoundStateManager manager, Collision collision) { }
    public override void OnTriggerExit(RoundStateManager manager, Collider other) { }

    public override void OnTriggerEnter(RoundStateManager manager, Collider other)
    {
        HandleArtifactCollision(manager, other);
        HandlePlayerCollision(manager, other);
        
    }

    private static void HandleArtifactCollision(RoundStateManager manager, Collider other)
    {
        // This handles if the artifact is thrown or the player walks in with an artifact
        if (other.gameObject.CompareTag("CanPickUp"))
        {
            ProcessArtifact(manager, other.GetComponent<IArtifact>());
        }
    }

    private static void HandlePlayerCollision(RoundStateManager manager, Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // If the player is holding an artifact.
            IArtifact heldArtifact = other.gameObject.GetComponentInChildren<IArtifact>();
            if (heldArtifact != null)
            {
                ProcessArtifact(manager, heldArtifact);
            }
        }
    }

    private static void ProcessArtifact(RoundStateManager manager, IArtifact artifact)
    {
        if (artifact != null && !artifact.hasBeenUsed)
        {
            manager.currentArtifacts++;
            artifact.hasBeenUsed = true; 
            manager.UpdateUI();
            
            if (manager.currentArtifacts >= manager.amountOfArtifactsToCompleteLevel)
            {
                manager.SwitchState(manager.GameOverState);
            }
        }
    }
}