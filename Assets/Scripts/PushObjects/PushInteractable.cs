using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushInteractable : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 45f;

    [Header("Physics Push & Tip Settings")]
    public float pushForce = 5f;
    public float timeToTipOver = 1.5f;
    public float tipTorqueForce = 15f;
    public float upwardPopForce = 2f;

    // Internal State Variables
    [HideInInspector] public bool pushPositive = false;
    [HideInInspector] public bool pushNegative = false;
    
    private Rigidbody rb;
    private Transform currentPusher;
    private float currentPushTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        HandleRotation();
        HandlePhysicsPush();
    }

    private void HandleRotation()
    {
        float inputDirection = 0f;
        
        // Calculate direction, cancelling out if both are true
        if (pushPositive && !pushNegative) inputDirection = 1f;
        else if (pushNegative && !pushPositive) inputDirection = -1f;

        if (inputDirection != 0f)
        {
            Vector3 rotationStep = rotationAxis.normalized * (inputDirection * rotationSpeed * Time.fixedDeltaTime);
            Quaternion deltaRotation = Quaternion.Euler(rotationStep);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }

    private void HandlePhysicsPush()
    {
        if (currentPusher == null) return;

        // Calculate force direction away from the pusher, ignoring Y axis
        Vector3 forceDirection = transform.position - currentPusher.position;
        forceDirection.y = 0;
        forceDirection.Normalize();

        // Apply standard push force
        rb.AddForce(forceDirection * pushForce, ForceMode.Force);

        // Tip over logic
        currentPushTime += Time.fixedDeltaTime;
        
        if (currentPushTime >= timeToTipOver)
        {
            Vector3 tipAxis = Vector3.Cross(Vector3.up, forceDirection);
            
            // Pop up slightly to clear ground friction, then apply torque to tip
            rb.AddForce(Vector3.up * upwardPopForce, ForceMode.VelocityChange);
            rb.AddTorque(tipAxis * tipTorqueForce, ForceMode.VelocityChange);
            
            currentPushTime = 0f; // Reset timer
        }
    }

    // Called by the detector to start/stop free pushing
    public void StartPhysicsPush(Transform pusher)
    {
        currentPusher = pusher;
    }

    public void StopPhysicsPush(Transform pusher)
    {
        if (currentPusher == pusher)
        {
            currentPusher = null;
            currentPushTime = 0f;
        }
    }
}