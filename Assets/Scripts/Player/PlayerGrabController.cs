using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerGrabController : MonoBehaviour
{
    [SerializeField] private Transform playerHand;
    [SerializeField] private float stackHeightOffset = 0.75f;

    [SerializeField] private bool allowWeightToAffectPlayerSpeed = false;
    
    private PlayerStealthController playerStealthController;
    private PlayerFootstepNoise playerFootstepNoise;

    public GameObject heldObject = null;

    private float currentHeldWeight = 0f;
    
    private void Awake()
    {
        playerStealthController = GetComponent<PlayerStealthController>();
        playerFootstepNoise = GetComponentInChildren<PlayerFootstepNoise>(); 
    }

    private void OnTriggerEnter(Collider other)
    {
        TryPickUpItem(other.gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        TryPickUpItem(other.gameObject);
    }

    private void TryPickUpItem(GameObject obj)
    {
        if (obj.CompareTag("CanPickUp"))
        {
            IPickable pickable = obj.GetComponent<IPickable>();
    
            if (pickable.CanBePickedUp && pickable.IsOnGround() && heldObject == null)
            {
                PickUpObject(obj, pickable);
            }
 
        }
    }



    private void PickUpObject(GameObject obj, IPickable pickable)
    {
        // Add item to our stack tracking
        heldObject = obj;

        pickable.OnPickedUp();
     
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
        
        // Re-apply the scale so it doesn't distort
        
        // not working -> maybe because the parent has a different scale?
        obj.transform.localScale = originalScale;

        // Cumulatively subtract speed for every item
        if (allowWeightToAffectPlayerSpeed)
        {
            playerStealthController.walkSpeed -= addedWeight;
            playerStealthController.sprintSpeed -= addedWeight;
        }
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
            
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.None; 
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

    public GameObject GetCurrentHeldItem()
    {
        return heldObject;
    }

    public void ReleaseTopObject()
    {
        IPickable pickable = heldObject.GetComponent<IPickable>();

        pickable.OnReleased();
        
        CheckAlarm(heldObject, false);
        
        float droppedWeight = 0f;

        // Maybe use a method to do this
        // Re-enable physics before dropping/throwing
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            droppedWeight = rb.mass;
            rb.isKinematic = false;
            
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate; 
        }

        // Re-enable colliders so it can bounce off the floor/walls again
        Collider[] colliders = heldObject.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        heldObject.transform.SetParent(null);
        
        // Re-add the individual object's weight back to our speed
        currentHeldWeight -= droppedWeight;
        
        if (allowWeightToAffectPlayerSpeed)
        {
            playerStealthController.walkSpeed += droppedWeight;
            playerStealthController.sprintSpeed += droppedWeight;
        }
  
        playerFootstepNoise.SetWeightModifier(currentHeldWeight);
        
        heldObject = null;
    }
}