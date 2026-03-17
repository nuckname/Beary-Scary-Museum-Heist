using UnityEngine;

public class DropSound : MonoBehaviour
{
    [SerializeField] private bool useDropSound = true;
    
    [Header("Stealth Propagation")]
    public LayerMask obstacleLayer; 
    public LayerMask enemyLayer;    

    private AudioSource audioSource;
    private BoxCollision boxData;

    [SerializeField] private float dropSoundMultiplier = 1f;
    private float dropSoundDisntance = 0f;
    
    public bool showVisibleRadius = true;
    public float visualDuration = 1.5f;
    public Material radiusMaterial;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        boxData = GetComponent<BoxCollision>();
        
        audioSource.spatialBlend = 1f; 
        audioSource.playOnAwake = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!useDropSound) return;
            
        dropSoundDisntance = boxData.boxWeight * dropSoundMultiplier;
        audioSource.maxDistance = dropSoundDisntance;
        
        // Play the dropping sound
        // idk for volume scale
        //audioSource.PlayOneShot(dropSound, 1);

        if (showVisibleRadius)
        {
            ShowRadiusInGame(dropSoundDisntance);
        }
        
        BroadcastSound(dropSoundDisntance);
    }
    
    private void BroadcastSound(float range)
    {
        // Find potential listeners in a radius
        Collider[] entitiesInHearingRange = Physics.OverlapSphere(transform.position, range, enemyLayer);

        // Raycast to check for walls
        foreach (Collider entity in entitiesInHearingRange)
        {
            Vector3 directionToEntity = entity.transform.position - transform.position;
            float distanceToEntity = directionToEntity.magnitude;

            if (!Physics.Raycast(transform.position, directionToEntity.normalized, distanceToEntity, obstacleLayer))
            {
                Debug.Log($"Sound reached {entity.name} through clear line of sight!");
            }
        }
    }
    
    //AI
    //https://gemini.google.com/share/cb1a8c33333e
    private void ShowRadiusInGame(float radius)
    {
        // 1. Create a basic 3D sphere
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        // 2. Destroy its collider immediately so it doesn't bump into your player or the floor
        Destroy(sphere.GetComponent<Collider>());

        // 3. Move it to the dropped object's position
        sphere.transform.position = transform.position;

        // 4. Scale it. A default sphere is 1 unit wide. We multiply the radius by 2 to get the diameter.
        float diameter = radius * 2f;
        sphere.transform.localScale = new Vector3(diameter, diameter, diameter);

        // 5. Apply your custom transparent material (if you assigned one)
        if (radiusMaterial != null)
        {
            sphere.GetComponent<Renderer>().material = radiusMaterial;
        }

        // 6. Tell Unity to destroy this sphere after 'visualDuration' seconds
        Destroy(sphere, visualDuration);
    }

    //AI
    //https://gemini.google.com/share/cb1a8c33333e
    // Draws a sphere in the Scene view when the object is selected
    private void OnDrawGizmosSelected()
    {
        // Safely fetch BoxCollision since Start() doesn't run in the Editor
        BoxCollision currentBoxData = GetComponent<BoxCollision>();
        
        if (currentBoxData != null)
        {
            // Set the color for the Gizmo (Cyan is usually easy to see)
            Gizmos.color = Color.cyan;
            
            // Draw a wireframe sphere using the boxWeight as the radius
            Gizmos.DrawWireSphere(transform.position, currentBoxData.boxWeight);
        }
    }
}