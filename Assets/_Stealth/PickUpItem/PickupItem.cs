using UnityEngine;

public class PickupItem : MonoBehaviour, IPickable
{
    [SerializeField] private float itemWeight = 2.0f;

    public float Weight
    {
        get
        {
            return itemWeight;
        }
    }

    public void OnPickedUp()
    {
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }
    }

    public void OnReleased()
    {
        // Re-enable physics when dropped
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
        }
    }
}