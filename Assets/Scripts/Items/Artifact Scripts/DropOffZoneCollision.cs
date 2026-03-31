using System;
using TMPro;
using UnityEngine;

public class DropOffZoneCollision : MonoBehaviour
{
    private TextMeshProUGUI artifactValueText;
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

                artifactValueText.text = "Points: " + totalQuota;
            }
        }
    }
}