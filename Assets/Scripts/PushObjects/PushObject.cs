using UnityEngine;

public class PushObject : MonoBehaviour
{
 [SerializeField] private float pushForce = 5f;
    
    [Header("Tip Over Settings")]
    [SerializeField] private float timeToTipOver = 1.5f; 
    [SerializeField] private float tipTorqueForce = 15f; 
    [SerializeField] private float upwardPopForce = 2f; // Helps flat objects clear ground friction

    private float currentPushTime = 0f;
    private GameObject currentPushedObject;
    private float lastContactTime = 0f; // Used to prevent micro-bounce resets

    private void Update()
    {
        // If we haven't touched the object in the last 0.2 seconds, it's safe to reset the timer
        // This prevents microscopic collision bounces from ruining our push timer!
        if (currentPushedObject != null && Time.time - lastContactTime > 0.2f)
        {
            currentPushedObject = null;
            currentPushTime = 0f;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Rigidbody rigidbody = collision.collider.attachedRigidbody;

        if (rigidbody != null && !rigidbody.isKinematic)
        {
            Vector3 forceDirection = collision.gameObject.transform.position - transform.position;
            forceDirection.y = 0;
            forceDirection.Normalize();

            rigidbody.AddForce(forceDirection * pushForce, ForceMode.Force);

            if (collision.gameObject == currentPushedObject)
            {
                currentPushTime += Time.fixedDeltaTime; 
                lastContactTime = Time.time; // Record the last exact moment we touched it
            }
            else
            {
                currentPushedObject = collision.gameObject;
                currentPushTime = 0f;
                lastContactTime = Time.time;
            }

            if (currentPushTime >= timeToTipOver)
            {
                Vector3 tipAxis = Vector3.Cross(Vector3.up, forceDirection);
                
                // Give it a tiny vertical hop so the edge doesn't snag on the ground friction
                rigidbody.AddForce(Vector3.up * upwardPopForce, ForceMode.VelocityChange);
                
                // Apply the torque to spin it
                rigidbody.AddTorque(tipAxis * tipTorqueForce, ForceMode.VelocityChange);
                
                currentPushTime = 0f; 
            }
        }
    }
}
