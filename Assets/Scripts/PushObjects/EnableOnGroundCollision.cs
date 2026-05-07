using UnityEngine;

public class EnableOnGroundCollision : MonoBehaviour
{
    private Collider myCollider;
    private Rigidbody myRigidbody;
    private CanPushObject canPushScript;

    void Start()
    {
        myCollider = GetComponent<Collider>();
        myRigidbody = GetComponent<Rigidbody>();
        canPushScript = GetComponent<CanPushObject>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            TurnOnComponents();
        }
    }

    private void TurnOnComponents()
    {
        myCollider.enabled = true;

        myRigidbody.isKinematic = false;

        canPushScript.enabled = true;
  
    }
}