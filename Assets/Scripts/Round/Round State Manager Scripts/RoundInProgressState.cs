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

    public override void OnCollisionEnter(RoundStateManager manager, Collision collision) { }
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
            IArtifact[] artifacts = other.GetComponents<IArtifact>();
            if (artifacts != null && artifacts.Length > 0)
            {
                ProcessArtifacts(manager, artifacts);
            }
        }
    }

    private static void HandlePlayerCollision(RoundStateManager manager, Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            IArtifact[] heldArtifacts = other.gameObject.GetComponentsInChildren<IArtifact>();
            if (heldArtifacts != null && heldArtifacts.Length > 0)
            {
                ProcessArtifacts(manager, heldArtifacts);
            }
        }
    }

    private static void ProcessArtifacts(RoundStateManager manager, IArtifact[] artifacts)
    {
        bool itemsProcessed = false;

        // Loop through every artifact passed in the array
        foreach (IArtifact artifact in artifacts)
        {
            if (artifact != null && !artifact.hasBeenUsed)
            {
                manager.currentArtifacts++;
                artifact.hasBeenUsed = true; 
                itemsProcessed = true;
            }
        }

        if (itemsProcessed)
        {
            manager.UpdateUI();
            
            if (manager.currentArtifacts >= manager.amountOfArtifactsToCompleteLevel)
            {
                manager.SwitchState(manager.GameOverState);
            }
        }
    }
}