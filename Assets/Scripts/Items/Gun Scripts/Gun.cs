using UnityEngine;

[RequireComponent(typeof(PickUpItem))] 
public class Gun : MonoBehaviour 
{
    [Header("Gun Settings")]
    public float bulletSpeed = 50f;
    public float noiseRadius = 10f;
    public float bulletFixedY = 1.5f;
    public LayerMask obstacleLayer;

    [Header("References")]
    public GameObject bulletPrefab;
    public Transform muzzlePoint;

    private PickUpItem basePickupItem;
    private bool hasBeenFired = false;
    private bool isHeld = false;

    private void Awake()
    {
        basePickupItem = GetComponent<PickUpItem>(); 
        
        basePickupItem.SetThrowableState(false); 
    }

    public void OnPickedUp()
    {
        isHeld = true;
    }

    public void OnReleased()
    {
        isHeld = false;
    }

    public bool IsOnGround()
    {
        return basePickupItem.IsOnGround();
    }

    void Update()
    {
        // Only shoot if held, not fired yet, and left-click is pressed
        if (isHeld && !hasBeenFired && Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.identity);
        Projectile projectile = bulletObj.GetComponent<Projectile>();
        
        projectile.Setup(
            muzzlePoint.position, 
            transform.forward, 
            bulletSpeed, 
            noiseRadius, 
            bulletFixedY, 
            obstacleLayer
        );

        hasBeenFired = true;
        
        // Tell the PickupItem that the gun is now empty and can be thrown!
        basePickupItem.SetThrowableState(true); 
    }
}