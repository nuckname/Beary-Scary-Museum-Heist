using UnityEngine;

public class DropSound : MonoBehaviour
{
    public LayerMask obstacleLayer; 
    public LayerMask enemyLayer;    

    private AudioSource audioSource;
    private BoxCollision boxData;

    [Tooltip("Multi for the sound radius, calculated against how much the object weights.")]
    [SerializeField] private float dropSoundMultiplier = 1f;
    
    private float dropSoundDisntance = 0f;
    
    [Header("2D Visual Circle")]
    [Tooltip("Show the flat circle.")]
    public bool showVisibleRadius = true;

    [Tooltip("duration of the flat circle before it disappears.")]
    public float visualDuration = 1.5f;
    
    [Tooltip("Material of the flat circle.")]
    public Material radiusMaterial;
    
    [Header("Velocity Settings")]
    [SerializeField] private bool useVelocityScaling = true;
    [Tooltip("How much the impact speed increases the sound multiplier.")]
    [SerializeField] private float velocityScale; 
    [Tooltip("Minimum impact speed required to generate a drop sound.")]
    [SerializeField] private float minVelocityThreshold;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        boxData = GetComponent<BoxCollision>();
        
        audioSource.spatialBlend = 1f; 
        audioSource.playOnAwake = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;

        float finalMultiplier = dropSoundMultiplier;
        float volumeScale = 1f;

        if (useVelocityScaling)
        {
            float impactVelocity = collision.relativeVelocity.magnitude;
        
            if (impactVelocity < minVelocityThreshold) return;

            finalMultiplier += (impactVelocity * velocityScale);
        
            // Maybe use velocity to also increase volume?
            //float volumeScale = Mathf.Clamp(impactVelocity / 10f, 0.1f, 1f);
        }

        dropSoundDisntance = boxData.boxWeight * finalMultiplier;
        audioSource.maxDistance = dropSoundDisntance;

        if (showVisibleRadius)
        {
            Vector3 hitPoint = collision.GetContact(0).point;
            ShowRadiusInGame(dropSoundDisntance, hitPoint);
        }
    
        BroadcastSound(dropSoundDisntance);
    }
    
    private void BroadcastSound(float baseRange)
    {
        // Find potential listeners in a radius
        Collider[] entitiesInHearingRange = Physics.OverlapSphere(transform.position, baseRange, enemyLayer);

        // Raycast to check for walls
        foreach (Collider entity in entitiesInHearingRange)
        {
            if (entity.GetComponentInChildren<ISoundListener>() is ISoundListener listener)
            {
                float finalRange = baseRange;
                
                if(entity.GetComponentInChildren<GuardHearing>() is GuardHearing ear)
                {
                    finalRange *= ear.hearingSensitivity;
                }

                Vector3 directionToEntity = entity.transform.position - transform.position;
                float distanceToEntity = directionToEntity.magnitude;

                // Make sure they are within their specific modified hearing range
                if (distanceToEntity <= finalRange)
                {
                    // Line of sight / acoustic occlusion check
                    if (!Physics.Raycast(transform.position, directionToEntity.normalized, out RaycastHit hit, distanceToEntity, obstacleLayer))
                    {
                        // reached the guard
                        Debug.DrawLine(transform.position, entity.transform.position, Color.green, 2f);

                        listener.OnSoundHeard(transform.position, transform);
                    }
                    else
                    {
                        // hit an obstacle. 
                        Debug.DrawLine(transform.position, hit.point, Color.red, 2f);
                    }
                }
                else
                {
                    // out of range
                    Debug.DrawLine(transform.position, entity.transform.position, Color.yellow, 2f);
                }
            }
        }
    }
    
    //AI
    //https://gemini.google.com/share/cb1a8c33333e
    //Updated to use 2D circle
    private void ShowRadiusInGame(float radius, Vector3 hitPosition)
    {
        // Create a Cylinder instead of a Sphere
        GameObject flatCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        
        // Destroy its collider immediately
        Destroy(flatCircle.GetComponent<Collider>());

        // Move it to the impact point. 
        // + 0.02f so its just above the ground to prevent flickering
        flatCircle.transform.position = hitPosition + new Vector3(0, 0.02f, 0);

        // Scale it. X and Z are the diameter. Y is squashed to make it a flat disc.
        float diameter = radius * 2f;
        flatCircle.transform.localScale = new Vector3(diameter, 0.01f, diameter);

        if (radiusMaterial != null)
        {
            flatCircle.GetComponent<Renderer>().material = radiusMaterial;
        }

        Destroy(flatCircle, visualDuration);
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollision currentBoxData = GetComponent<BoxCollision>();
        
        if (currentBoxData != null)
        {
            Gizmos.color = Color.cyan;
            float radius = currentBoxData.boxWeight * dropSoundMultiplier;

            // Draw a flat wire circle in the editor instead of a 3D sphere
            int segments = 36;
            float angle = 0f;
            Vector3 lastPoint = transform.position + new Vector3(radius, 0, 0);
            
            for (int i = 1; i <= segments; i++)
            {
                angle += (360f / segments) * Mathf.Deg2Rad;
                Vector3 nextPoint = transform.position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(lastPoint, nextPoint);
                lastPoint = nextPoint;
            }
        }
    }
}