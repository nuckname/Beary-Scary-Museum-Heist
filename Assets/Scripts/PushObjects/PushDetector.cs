using UnityEngine;

// https://www.youtube.com/watch?v=3BOn2gs7z04

[RequireComponent(typeof(Collider))]
public class PushDetector : MonoBehaviour
{
    public enum InteractionType 
    { 
        RotatePositive, 
        RotateNegative, 
        PhysicsPush 
    }

    public PushInteractable pushInteractable;

    [Tooltip("Choose what happens when an entity walks into this trigger.")]
    public InteractionType interactionType = InteractionType.RotatePositive;

    [Header("Filtering")]
    [Tooltip("Tag of the object allowed to push. Leave empty to rely only on layers.")]
    public string pusherTag = "Player";
    
    [Tooltip("Layers that are allowed to push the object.")]
    public LayerMask pushableLayers;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsValidPusher(other.gameObject)) return;

        switch (interactionType)
        {
            case InteractionType.RotatePositive:
                pushInteractable.pushPositive = true;
                break;
            case InteractionType.RotateNegative:
                pushInteractable.pushNegative = true;
                break;
            case InteractionType.PhysicsPush:
                pushInteractable.StartPhysicsPush(other.transform);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidPusher(other.gameObject)) return;

        switch (interactionType)
        {
            case InteractionType.RotatePositive:
                pushInteractable.pushPositive = false;
                break;
            case InteractionType.RotateNegative:
                pushInteractable.pushNegative = false;
                break;
            case InteractionType.PhysicsPush:
                pushInteractable.StopPhysicsPush(other.transform);
                break;
        }
    }

    private bool IsValidPusher(GameObject obj)
    {
        bool matchesTag = !string.IsNullOrEmpty(pusherTag) && obj.CompareTag(pusherTag);

        // Layer check
        // Get the integer index of the object's layer
        int objectLayerIndex = obj.layer;

        // Convert that index into a bitmask by shifting '1' to the left by that many spaces
        int objectLayerMask = 1 << objectLayerIndex;

        // Get the raw integer value of the allowed layers you selected in the Inspector
        int allowedLayersValue = pushableLayers.value;

        int overlappingBits = allowedLayersValue & objectLayerMask;

        bool matchesLayer = overlappingBits != 0;
        
        return matchesTag || matchesLayer;
    }
}