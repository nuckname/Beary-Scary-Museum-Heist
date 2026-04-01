using UnityEngine;

public class IsArtifact : MonoBehaviour, IArtifact
{
    // The [field: SerializeField] attribute forces Unity to show these interface properties 
    // in the Inspector so you can set the artifactValue manually for different items.
    [field: SerializeField] public bool hasBeenUsed { get; set; }

}