using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Gun : CanPickUpItem
{
    [Header("Gun Settings")]
    public float bulletSpeed = 50f;
    public float noiseRadius = 10f;
    public float bulletFixedY = 1.5f;
    public LayerMask obstacleLayer;

    [Header("Laser Sight Settings")]
    public int maxBounces = 3;
    public float maxLaserDistance = 50f;
    public bool force45DegreeBounce = false; 

    [Header("References")]
    public GameObject bulletPrefab;
    public Transform muzzlePoint;

    private LineRenderer lineRenderer;
    private bool hasBeenFired = false;
    private bool isHeld = false;

    protected override void Awake() 
    {
        base.Awake(); 
        
        SetThrowableState(false); 

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    public override void OnPickedUp()
    {
        base.OnPickedUp();
        
        isHeld = true;
        
        // Only prevent throwing if the gun hasn't been fired yet
        if (!hasBeenFired)
        {
            SetThrowableState(false);
            lineRenderer.enabled = true;
        }
        else
        {
            // Keep it throwable if we pick up an empty gun
            SetThrowableState(true);
        }
    }

    public override void OnReleased()
    {
        base.OnReleased(); 
        
        isHeld = false;
        lineRenderer.enabled = false; 
    }

    void Update()
    {
        if (isHeld && !hasBeenFired)
        {
            DrawLaser();

            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
    }

    private void DrawLaser()
    {
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, muzzlePoint.position);

        Vector3 currentPos = muzzlePoint.position;
        Vector3 currentDir = transform.forward;
        int positionIndex = 1;

        for (int i = 0; i <= maxBounces; i++)
        {
            if (Physics.Raycast(currentPos, currentDir, out RaycastHit hit, maxLaserDistance, obstacleLayer))
            {
                lineRenderer.positionCount = positionIndex + 1;
                lineRenderer.SetPosition(positionIndex, hit.point);

                if (force45DegreeBounce)
                {
                    Vector3 cross = Vector3.Cross(hit.normal, Vector3.up);
                    currentDir = Quaternion.AngleAxis(45f, cross) * hit.normal;
                }
                else
                {
                    currentDir = Vector3.Reflect(currentDir, hit.normal);
                }

                currentPos = hit.point + (currentDir * 0.01f);
                positionIndex++;
            }
            else
            {
                lineRenderer.positionCount = positionIndex + 1;
                lineRenderer.SetPosition(positionIndex, currentPos + (currentDir * maxLaserDistance));
                break; 
            }
        }
    }

    private void Shoot()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.identity);
        Projectile projectile = bulletObj.GetComponent<Projectile>();
    
        Vector3[] pathPoints = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(pathPoints);

        projectile.Setup(
            pathPoints, 
            bulletSpeed, 
            noiseRadius, 
            bulletFixedY, 
            obstacleLayer
        );

        hasBeenFired = true;
        lineRenderer.enabled = false;
    
        SetThrowableState(true); 
    }
}