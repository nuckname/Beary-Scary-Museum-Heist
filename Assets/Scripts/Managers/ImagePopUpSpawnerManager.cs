using UnityEngine;

public class ImagePopUpSpawnerManager : MonoBehaviour
{
    public static ImagePopUpSpawnerManager Instance;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject[] impactPrefabs;

    [Header("Position Settings")]
    [SerializeField] private float positionOffsetRadius = 0.75f;

    [Header("Rotation Settings")]
    [SerializeField] private bool applyRandomRotation = true;
    [SerializeField] private float minRotationZ = -45f;
    [SerializeField] private float maxRotationZ = 45f;

    [Header("Scale Settings")]
    [SerializeField] private bool applyRandomScale = true;
    [SerializeField] private float minScale = 0.3f;
    [SerializeField] private float maxScale = 0.55f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnRandomImpact(Vector3 position)
    {
        if (impactPrefabs == null || impactPrefabs.Length == 0) 
        {
            Debug.LogWarning("Impact Prefabs array is empty!");
            return;
        }

        GameObject prefabToSpawn = impactPrefabs[Random.Range(0, impactPrefabs.Length)];
        
        Vector3 randomOffset = Vector3.zero;
        randomOffset = (Vector3)Random.insideUnitCircle * positionOffsetRadius;
    

        Vector3 finalPosition = position + randomOffset;


        Quaternion finalRotation = Quaternion.identity;
        if (applyRandomRotation)
        {
            float randomZ = Random.Range(minRotationZ, maxRotationZ);
            finalRotation = Quaternion.Euler(0f, 0f, randomZ);
        }


        GameObject spawnedImpact = Instantiate(prefabToSpawn, finalPosition, finalRotation);


        if (applyRandomScale)
        {
            float randomUniformScale = Random.Range(minScale, maxScale);
            spawnedImpact.transform.localScale = new Vector3(randomUniformScale, randomUniformScale, randomUniformScale);
        }
    }
}