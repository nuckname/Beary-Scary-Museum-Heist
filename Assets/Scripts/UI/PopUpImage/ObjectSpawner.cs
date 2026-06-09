using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Position Range")]
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(2f, 0f, 2f);

    [Header("Rotation Range")]
    [SerializeField] private Vector3 minRotation;
    [SerializeField] private Vector3 maxRotation = new Vector3(0f, 360f, 0f);

    [Header("Scale Range")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.5f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnObject();
            timer = 0f;
        }
    }

    private void SpawnObject()
    {
        Vector3 randomPosition = transform.position + new Vector3(
            Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f),
            Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
        );

        Quaternion randomRotation = Quaternion.Euler(
            Random.Range(minRotation.x, maxRotation.x),
            Random.Range(minRotation.y, maxRotation.y),
            Random.Range(minRotation.z, maxRotation.z)
        );

        GameObject spawnedObj = Instantiate(prefabToSpawn, randomPosition, randomRotation);

        float randomScale = Random.Range(minScale, maxScale);
        spawnedObj.transform.localScale = Vector3.one * randomScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, spawnAreaSize);
    }
}