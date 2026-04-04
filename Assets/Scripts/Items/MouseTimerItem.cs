using UnityEngine;

[RequireComponent(typeof(NoiseEmitter))]
public class MouseTimerItem : CanPickUpItem, IThrowableItem 
{
    private NoiseEmitter noiseEmitter;

    [Header("Movement Settings")]
    [Tooltip("How fast the item moves forward.")]
    public float moveSpeed = 3f;
    
    [Header("Noise Settings")]
    [Tooltip("How often the noise triggers (in seconds).")]
    public float noiseInterval = 0.5f;
    
    [Tooltip("The radius of the emitted noise to alert Guards/AI.")]
    public float noiseRadius = 4f;

    private float timer = 5f;
    
    [SerializeField] private bool isArmed = false;
    [SerializeField] private bool isActivated = false;

    protected override void Awake()
    {
        // Grabs the Rigidbody and Collider from the parent script
        base.Awake(); 
        noiseEmitter = GetComponent<NoiseEmitter>();
    }

    public override void OnPickedUp()
    {
        base.OnPickedUp();

        isActivated = false;
        isArmed = true;

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    public override void OnReleased()
    {
        base.OnReleased(); // Turns gravity/kinematics back to normal
    }

    public void OnThrown()
    {
        isArmed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;

        if (isArmed)
        {
            isArmed = false;
            isActivated = true;
            timer = 0f; 

            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
            }

            Vector3 flatEuler = transform.eulerAngles;
            flatEuler.x = 0;
            flatEuler.z = 0;
            transform.eulerAngles = flatEuler;
        }
        else if (isActivated)
        {
            ContactPoint contact = collision.GetContact(0);
            Vector3 reflectedDirection = Vector3.Reflect(transform.forward, contact.normal);
            reflectedDirection.y = 0f;

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
        noiseEmitter.EmitNoise(noiseRadius, NoiseType.Item);
    }
}