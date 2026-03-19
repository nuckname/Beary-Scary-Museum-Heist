using UnityEngine;

[RequireComponent(typeof(NoiseEmitter))]
[RequireComponent(typeof(Rigidbody))]
public class NoiseTimerMoveForwardItem : MonoBehaviour, IThrowableItem
{
    private NoiseEmitter noiseEmitter;
    private Rigidbody rb;

    [Header("Movement Settings")]
    [Tooltip("How fast the item moves forward.")]
    public float moveSpeed = 3f;
    
    [Header("Noise Settings")]
    [Tooltip("How often the noise triggers (in seconds).")]
    public float noiseInterval = 0.5f;
    
    [Tooltip("The radius of the emitted noise to alert Guards /ai.")]
    public float noiseRadius = 4f;

    private float timer = 0f;
    
    // IThrowableItem variables
    private bool isArmed = false;
    private bool isActivated = false;

    private void Start()
    {
        noiseEmitter = GetComponent<NoiseEmitter>();
        rb = GetComponent<Rigidbody>();
    }

    // Called by the PlayerThrowController via the interface
    public void OnThrown()
    {
        isArmed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isArmed && !collision.gameObject.CompareTag("Player"))
        {
            isArmed = false;
            isActivated = true;
            timer = 0f; // Trigger the first noise immediately on impact

            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
                //rb.isKinematic = true; 
            }

            // Flatten the rotation so it drives nicely along the floor sort of parallel to the ground
            Vector3 flatEuler = transform.eulerAngles;
            flatEuler.x = 0;
            flatEuler.z = 0;
            transform.eulerAngles = flatEuler;
        }
        else if (isActivated)
        {
            //AI
            //https://gemini.google.com/share/aa687b042cde
            
            // Get the first contact point to find out which way the wall/object is facing
            ContactPoint contact = collision.GetContact(0);

            // Calculate the bounce direction by reflecting our current forward vector against the wall's normal
            Vector3 reflectedDirection = Vector3.Reflect(transform.forward, contact.normal);

            // Flatten the Y axis so the bounce keeps it driving along the floor, not up into the air
            reflectedDirection.y = 0f;

            // Rotate the object to face the newly calculated bounce direction
            if (reflectedDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(reflectedDirection.normalized);
            }
        }
    }

    private void Update()
    {
        if (!isActivated) return;

        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime));

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            TriggerNoise();
            timer = noiseInterval; 
        }
    }

    private void TriggerNoise()
    {
        noiseEmitter.EmitNoise(noiseRadius, transform.position);
        
        // Call audio here
    }
}