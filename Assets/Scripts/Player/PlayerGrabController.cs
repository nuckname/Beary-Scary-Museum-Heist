using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerGrabController : MonoBehaviour
{
    [SerializeField] private Transform playerHand;
    [SerializeField] private float stackHeightOffset = 0.75f; 
    
    private PlayerStealthController playerStealthController;
    private PlayerFootstepNoise playerFootstepNoise;
    
    public List<GameObject> HeldObjects { get; private set; } = new List<GameObject>();

    private float currentHeldWeight = 0f;
    
    private void Awake()
    {
        playerStealthController = GetComponent<PlayerStealthController>();
        playerFootstepNoise = GetComponentInChildren<PlayerFootstepNoise>(); 
    }

    private void OnCollisionEnter(Collision other)
    {
        // Make sure we only pick it up if it's not already in our stack
        if (other.gameObject.CompareTag("CanPickUp") && !HeldObjects.Contains(other.gameObject))
        {
            IPickable[] pickables = other.gameObject.GetComponents<IPickable>();
        
            if (pickables.Length > 0)
            {
                if (pickables[0].CanBePickedUp && pickables[0].IsOnGround())
                {
                    PickUpObject(other.gameObject, pickables);
                }
            }
            else
            {
                Debug.LogError("pickables length is 0");
            }
        }
    }

    private void PickUpObject(GameObject obj, IPickable[] pickables)
    {
        // Add item to our stack tracking
        HeldObjects.Add(obj);

        foreach (IPickable pickable in pickables)
        {
            pickable.OnPickedUp();
        }
        
        DestroyGameobjects(obj);

        CheckAlarm(obj, true);
        
        float addedWeight = AddWeight(obj);

        DisableAllColliders(obj);

        playerFootstepNoise.SetWeightModifier(currentHeldWeight);

        SetUpPlayerHand(obj, addedWeight);
    }

    private void SetUpPlayerHand(GameObject obj, float addedWeight)
    {
        // Store the object's original world scale before parenting
        Vector3 originalScale = obj.transform.localScale;
        
        // Attach to hand
        obj.transform.SetParent(playerHand);
        
        // Stack the object higher depending on how many items we are currently holding
        float heightOffset = (HeldObjects.Count - 1) * stackHeightOffset;
        obj.transform.localPosition = new Vector3(0, heightOffset, 0);
        obj.transform.localRotation = Quaternion.identity;
        
        // Re-apply the scale so it doesn't distort
        obj.transform.localScale = originalScale;

        // Cumulatively subtract speed for every item
        playerStealthController.walkSpeed -= addedWeight;
        playerStealthController.sprintSpeed -= addedWeight;
    }

    private float AddWeight(GameObject obj)
    {
        float addedWeight = 0f;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            addedWeight = rb.mass;
            currentHeldWeight += addedWeight;
            
            rb.isKinematic = true; 
        }

        return addedWeight;
    }

    // Disable colliders so it doesn't push against the player character
    private void DisableAllColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    // Current in use for the painting so it can float.
    private void DestroyGameobjects(GameObject obj)
    {
        IDestroyOnGrab destroyableComponent = obj.GetComponentInChildren<IDestroyOnGrab>();
        if (destroyableComponent != null)
        {
            // Destroy only the child game object that has the script
            Destroy(destroyableComponent.gameObject);
        }
    }

    private void CheckAlarm(GameObject obj, bool turnOnAlarm)
    {
        if(obj.TryGetComponent(out IsKey key))
        {
            if (key.IsAlarmKey)
            {
                if (turnOnAlarm)
                {
                    obj.GetComponent<AlarmComponent>().StartAlarm();
                }
                else
                {
                    obj.GetComponent<AlarmComponent>().StopAlarm();
                }
            }
        }
    }

    public GameObject GetTopObject()
    {
        if (HeldObjects.Count == 0) return null;
        
        // The last object added to the list
        return HeldObjects[HeldObjects.Count - 1]; 
    }

    public void ReleaseTopObject()
    {
        if (HeldObjects.Count == 0) return;

        GameObject objectToDrop = HeldObjects[HeldObjects.Count - 1];
        IPickable[] pickables = objectToDrop.GetComponents<IPickable>();

        if (pickables != null)
        {
            foreach (IPickable pickable in pickables)
            {
                pickable.OnReleased();
            }
        }
        
        CheckAlarm(objectToDrop, false);
        

        float droppedWeight = 0f;

        // Maybe use a method to do this
        // Re-enable physics before dropping/throwing
        Rigidbody rb = objectToDrop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            droppedWeight = rb.mass;
            rb.isKinematic = false;
        }

        // Re-enable colliders so it can bounce off the floor/walls again
        Collider[] colliders = objectToDrop.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        objectToDrop.transform.SetParent(null);
        
        // Re-add the individual object's weight back to our speed
        currentHeldWeight -= droppedWeight;
        playerStealthController.walkSpeed += droppedWeight;
        playerStealthController.sprintSpeed += droppedWeight;
  
        playerFootstepNoise.SetWeightModifier(currentHeldWeight);
      
        // Finally remove it from the stack
        HeldObjects.RemoveAt(HeldObjects.Count - 1);
    }
}