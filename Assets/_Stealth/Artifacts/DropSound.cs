using UnityEngine;

public class DropSound : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip dropSound;
    
    [Header("Stealth Propagation")]
    public LayerMask obstacleLayer; 
    public LayerMask enemyLayer;    

    private AudioSource audioSource;
    private BoxCollision boxData;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        boxData = GetComponent<BoxCollision>();
        
        audioSource.spatialBlend = 1f; 
        audioSource.playOnAwake = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        audioSource.maxDistance = boxData.boxWeight;
        
        // Play the dropping sound
        // idk for volume scale
        audioSource.PlayOneShot(dropSound, 1);

        BroadcastSound(boxData.boxWeight);
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
}