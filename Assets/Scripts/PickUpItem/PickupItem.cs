using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PickupItem : MonoBehaviour, IPickable
{
    [Header("Base Pickup Settings")]
    [SerializeField] protected float itemWeight = 2.0f;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected float groundCheckBuffer = 3f;

    protected Rigidbody rb;
    protected Collider col;

    public float Weight => itemWeight;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public virtual void OnPickedUp()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public virtual void OnReleased()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    // Was going to use ground layer but it's not working so as longs as it's something thats not that air,
    // it's not the ground
    public virtual bool IsOnGround()
    {
        if (col == null) return true;

        Vector3 origin = col.bounds.center;
        float maxDistance = col.bounds.extents.y + groundCheckBuffer;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, maxDistance))
        {
            if (!hitInfo.collider.isTrigger && !hitInfo.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }
}