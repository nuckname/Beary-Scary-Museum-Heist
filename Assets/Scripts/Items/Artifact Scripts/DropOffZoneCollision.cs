using System;
using TMPro;
using UnityEngine;

public class DropOffZoneCollision : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI artifactValueText;
    
    public int totalQuota = 0;

    private void Awake()
    {
        artifactValueText = GameObject.FindGameObjectWithTag("artifactValueText").GetComponent<TextMeshProUGUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CanPickUp"))
        {
            IArtifact artifact = other.GetComponent<IArtifact>();
            
            if (artifact != null && !artifact.hasBeenUsed)
            {
                totalQuota += artifact.artifactValue;
                
                artifact.hasBeenUsed = true;

                Debug.Log($"value: {artifact.artifactValue} added to {totalQuota}");
                artifactValueText.text = "Quota: " + artifact.artifactValue + " / Max quota";
                // Destroy(other.gameObject);
            }
        }
    }
}