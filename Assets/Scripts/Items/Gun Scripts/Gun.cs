using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Settings")]
    public float bulletSpeed = 50f;
    public float noiseRadius = 10f;
    public float bulletFixedY = 1.5f;
    public LayerMask obstacleLayer;

    [Header("References")]
    public GameObject bulletPrefab;
    public Transform muzzlePoint;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.identity);
        Projectile projectile = bulletObj.GetComponent<Projectile>();
        
        // Pass the direction the gun is facing
        projectile.Setup(
            muzzlePoint.position, 
            transform.forward, 
            bulletSpeed, 
            noiseRadius, 
            bulletFixedY, 
            obstacleLayer
        );
    }
}