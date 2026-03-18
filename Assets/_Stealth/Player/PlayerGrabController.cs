using UnityEngine;

public class PlayerGrabController : MonoBehaviour
{
    [SerializeField] private Transform playerHand;
    private PlayerStealthController playerStealthController;

    public GameObject PickedUpObject { get; private set; }
    public IPickable CurrentPickableData { get; private set; }

    private void Awake()
    {
        playerStealthController = GetComponent<PlayerStealthController>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("CanPickUp") && PickedUpObject == null)
        {
            if (hit.gameObject.TryGetComponent<IPickable>(out var pickable))
            {
                PickUpObject(hit.gameObject, pickable);
            }
        }
    }

    private void PickUpObject(GameObject obj, IPickable pickable)
    {
        PickedUpObject = obj;
        CurrentPickableData = pickable;

        pickable.OnPickedUp();

        // Attach to hand
        obj.transform.SetParent(playerHand);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        playerStealthController.walkSpeed -= pickable.Weight;
        playerStealthController.sprintSpeed -= pickable.Weight;
    }

    public void ReleaseObject()
    {
        if (PickedUpObject == null) return;

        CurrentPickableData.OnReleased();

        PickedUpObject.transform.SetParent(null);
        
        if (CurrentPickableData != null)
        {
            playerStealthController.walkSpeed += CurrentPickableData.Weight;
            playerStealthController.sprintSpeed += CurrentPickableData.Weight;
        }

        PickedUpObject = null;
        CurrentPickableData = null;
    }
}