using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class RoundInProgressState : RoundBaseState
{
    private bool playerIsInGreenZone = false;
    public override void EnterState(RoundStateManager manager)
    {
        Debug.Log("Round is in progress.");

        //manager.timer.isPaused = false;

        manager.MakeGuardsStartMoving();
    }

    public override void UpdateState(RoundStateManager manager)
    {
    }

    public override void OnCollisionEnter(RoundStateManager manager, Collision collision)
    {
    }

    public override void OnCollisionExit(RoundStateManager manager, Collision collision)
    {
    }

    public override void OnTriggerEnter(RoundStateManager manager, Collider other)
    {
        HandlePlayerCollision(manager, other);

        HandleArtifactCollision(manager, other);
    }

    private static void HandleArtifactCollision(RoundStateManager manager, Collider other)
    {
        if (other.gameObject.CompareTag("CanPickUp"))
        {
            IArtifact artifact = other.GetComponent<IArtifact>();
        
            if (artifact != null)
            {
                if (!artifact.hasBeenUsed)
                {
                    manager.currentArtifacts++;
                    
                    artifact.hasBeenUsed = true; 
                    
                    manager.UpdateUI();
                }
            }
        }
    }

    private static void HandlePlayerCollision(RoundStateManager manager, Collider other)
    {
        // If player is in zone and we have all artifacts collected go to next round
        if (other.gameObject.CompareTag("Player"))
        {
            if(manager.currentArtifacts == manager.amountOfArtifactsToCompleteLevel)
            {
                // Round completed text? Pop up?
                manager.SwitchState(manager.AboutToStartState);
            }
        }
    }

    public override void OnTriggerExit(RoundStateManager manager, Collider other)
    {
    }  
}