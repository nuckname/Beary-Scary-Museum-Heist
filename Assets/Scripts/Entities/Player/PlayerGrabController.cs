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
            IPickable[] pickables = obj.GetComponents<IPickable>();

            if (pickables.Length > 0 && pickables[0].CanBePickedUp && pickables[0].IsOnGround() && heldObject == null)
            {
                PickUpObject(obj, pickables);
            }
            else if (pickables.Length == 0)
            {
                UnityEngine.Debug.LogWarning($"GameObject '{obj.name}' is tagged 'CanPickUp' but is missing an IPickable script!");
            }
        }
    }



    private void PickUpObject(GameObject obj, IPickable[] pickables)
    {
        // Add item to our stack tracking
        heldObject = obj;

        // An item can have mutiple pick up effects or conditions so it needs to be called multiple times. 
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
    	obj.transform.SetParent(playerHand, true);
    
    	obj.transform.localPosition = Vector3.zero;
    	obj.transform.localRotation = Quaternion.identity;

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