using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed;
    private float noiseRadius;
    private float forcedY;
    private LayerMask obstacleLayer;
    
   [SerializeField]  private NoiseEmitter emitter;

    private Vector3[] pathPoints;
    private int currentPointIndex = 1; 

    public void Setup(Vector3[] points, float bulletSpeed, float radius, float yLevel, LayerMask mask)
    {
        pathPoints = points;
        speed = bulletSpeed;
        noiseRadius = radius;
        forcedY = yLevel;
        obstacleLayer = mask;

        // Start at the exact muzzle position
        transform.position = new Vector3(pathPoints[0].x, forcedY, pathPoints[0].z);
    }

    void Update()
    {
        if (pathPoints == null || pathPoints.Length == 0 || currentPointIndex >= pathPoints.Length) return;

        Vector3 targetPoint = pathPoints[currentPointIndex];
        targetPoint.y = forcedY; 

        float moveStep = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, moveStep);

        if (Vector3.Distance(transform.position, targetPoint) < 0.01f)
        {
            CreateNoise(transform.position);

            currentPointIndex++; 

            if (currentPointIndex >= pathPoints.Length)
            {
                Destroy(gameObject);
            }
        }
        
        if (Vector3.Distance(Vector3.zero, transform.position) > 500f) 
        {
            Destroy(gameObject);
        }
    }

    private void CreateNoise(Vector3 impactPoint)
    {
        
        //emitter.EmitNoise(noiseRadius, NoiseType.Item); 
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;
        
        if (other.CompareTag("Enemy"))
        {
            CreateNoise(transform.position);
            Destroy(gameObject);
        }
    }
}