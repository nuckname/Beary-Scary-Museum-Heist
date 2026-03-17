using UnityEngine;

public class PlayerGrabController : MonoBehaviour
{
    [SerializeField] private Transform playerHand;
    private PlayerStealthController playerStealthController;

    public GameObject PickedUpObject { get; private set; }
    public BoxCollision CurrentBoxData { get; private set; }

    private void Awake()
    {
        playerStealthController = GetComponent<PlayerStealthController>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Box") && PickedUpObject == null)
        {
            BoxCollision box = hit.gameObject.GetComponent<BoxCollision>();
            if (box != null)
            {
                PickUpObject(hit.gameObject, box);
            }
        }
    }

    private void PickUpObject(GameObject collideBoxObject, BoxCollision boxCollision)
    {
        PickedUpObject = collideBoxObject;
        CurrentBoxData = boxCollision;

        Rigidbody boxRb = PickedUpObject.GetComponent<Rigidbody>();
        if (boxRb != null) boxRb.isKinematic = true;

        Collider boxCollider = PickedUpObject.GetComponent<Collider>();
        if (boxCollider != null) boxCollider.enabled = true;

        collideBoxObject.transform.SetParent(playerHand);
        collideBoxObject.transform.localPosition = Vector3.zero;
        collideBoxObject.transform.localRotation = Quaternion.identity;

        playerStealthController.moveSpeed -= boxCollision.boxWeight;
    }

    public void ReleaseObject()
    {
        if (PickedUpObject == null) return;

        PickedUpObject.transform.SetParent(null);

        BoxCollider boxCollider = PickedUpObject.GetComponent<BoxCollider>();
        if (boxCollider != null) boxCollider.enabled = true;

        if (CurrentBoxData != null)
        {
            playerStealthController.moveSpeed += CurrentBoxData.boxWeight;
        }

        PickedUpObject = null;
        CurrentBoxData = null;
    }
}