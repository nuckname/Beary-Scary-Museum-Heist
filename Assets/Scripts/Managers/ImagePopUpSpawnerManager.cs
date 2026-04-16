using UnityEngine;

public class ImagePopUpSpawnerManager : MonoBehaviour
{
    public static ImagePopUpSpawnerManager Instance;
    
    [SerializeField] private GameObject[] impactPrefabs;

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
        GameObject prefabToSpawn = impactPrefabs[Random.Range(0, impactPrefabs.Length)];
        
        Instantiate(prefabToSpawn, position, Quaternion.identity);
    }
}
