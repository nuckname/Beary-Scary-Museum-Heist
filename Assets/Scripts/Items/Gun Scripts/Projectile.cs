using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed;
    private float noiseRadius;
    private float forcedY;
    private Vector3 direction;
    private LayerMask obstacleLayer;

    // Initialize the bullet properties
    public void Setup(Vector3 startPos, Vector3 dir, float bulletSpeed, float radius, float yLevel, LayerMask mask)
    {
        direction = dir;
        speed = bulletSpeed;
        noiseRadius = radius;
        forcedY = yLevel;
        obstacleLayer = mask;

        // Force initial y level
        transform.position = new Vector3(startPos.x, forcedY, startPos.z);
    }

    void Update()
    {
        // Move the bullet
        Vector3 moveStep = direction * speed * Time.deltaTime;
        Vector3 nextPosition = transform.position + moveStep;
        
        // Keep it at the fixed Y level
        nextPosition.y = forcedY;

        // Check for collision with obstacles
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, moveStep.magnitude, obstacleLayer))
        {
            HandleImpact(hit.point);
        }

        transform.position = nextPosition;

        // Cleanup if it goes too far
        if (Vector3.Distance(Vector3.zero, transform.position) > 500f) Destroy(gameObject);
    }

    private void HandleImpact(Vector3 impactPoint)
    {
        GameObject impactObj = new GameObject("BulletImpactNoise");
        impactObj.transform.position = impactPoint;
        
        NoiseEmitter emitter = impactObj.AddComponent<NoiseEmitter>();
        
        emitter.EmitNoise(noiseRadius, NoiseType.Item); 

        Destroy(gameObject);
    }
}