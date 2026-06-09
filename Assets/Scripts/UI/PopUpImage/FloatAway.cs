using UnityEngine;

public class FloatAway : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float floatSpeed = 3f;
    [SerializeField] private Vector3 floatDirection = Vector3.up;
    
    [Header("Lifecycle Settings")]
    [Tooltip("How long in seconds before the object destroys itself.")]
    [SerializeField] private float lifetime = 5f;

    private Vector3 initialScale;
    private float age;

    private void Start()
    {
        initialScale = transform.localScale;
        
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(floatDirection.normalized * (floatSpeed * Time.deltaTime), Space.World);

        age += Time.deltaTime;
        
        float lifePercentage = age / lifetime;
        
        transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, lifePercentage);
    }
}