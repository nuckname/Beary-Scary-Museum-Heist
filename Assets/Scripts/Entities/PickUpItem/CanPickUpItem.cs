using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class CanPickUpItem : MonoBehaviour, IPickable, IThrowableItem
{
    [Header("Base Pickup Settings")]
    [SerializeField] protected float itemWeight = 2.0f;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected float groundCheckBuffer = 3f;

    [Header("Throw Settings")]
    [SerializeField] protected bool canBeThrown = true; 
    public ItemType typeOfItem = ItemType.None;
    
    private ItemImage itemImage;
    
    protected Rigidbody rb;
    protected Collider col;
    
    protected bool isAirborne = false;

    public float Weight => itemWeight;
    public bool CanThrowItem => canBeThrown; 
    public ItemType ItemType => typeOfItem;
    
    public virtual bool CanBePickedUp => !isAirborne;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        itemImage = GetComponent<ItemImage>();
    }

    public virtual void OnPickedUp()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        itemImage.spriteRenderer.enabled = false;
        
        isAirborne = false; 
    }
    
    public void SetThrowableState(bool state)
    {
        canBeThrown = state;
    }

    public virtual void OnReleased()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
    
    public void OnThrown(Vector3 velocity)
    {
        if (!CanThrowItem) 
        {
            return; 
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = velocity; 

        isAirborne = true;

        Debug.Log($"{gameObject.name} was thrown. speed {velocity.magnitude}");
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (isAirborne)
        {
            if (!collision.gameObject.CompareTag("Player"))
            {
                isAirborne = false;
            }
        }
    }

    public virtual bool IsOnGround()
    {
        if (col == null) return true;

        Vector3 origin = col.bounds.center;
        float maxDistance = col.bounds.extents.y + groundCheckBuffer;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, maxDistance))
        {
            if (!hitInfo.collider.isTrigger && !hitInfo.collider.CompareTag("Player"))
            {
                // We want to show the icon again if we hit the ground.
                if (itemImage != null)
                {
                    itemImage.allowImageToShow = true;
                }
                return true;
            }
        }

        return false;
    } 
}