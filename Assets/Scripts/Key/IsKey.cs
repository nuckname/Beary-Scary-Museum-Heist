using UnityEngine;

public class IsKey : MonoBehaviour, IKey
{
    [field: SerializeField] public bool hasBeenUsed { get; set; }

}