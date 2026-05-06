using UnityEngine;

// https://www.youtube.com/watch?v=3BOn2gs7z04
public class CanPushObject : MonoBehaviour
{
    [SerializeField] private float pushForce = 5f;

    private void OnCollisionStay(Collision collision)
    {
        Rigidbody rigidbody = collision.collider.attachedRigidbody;

        if (rigidbody != null && !rigidbody.isKinematic)
        {
            Vector3 forceDirection = collision.gameObject.transform.position - transform.position;
            forceDirection.y = 0;
            forceDirection.Normalize();

            rigidbody.AddForce(forceDirection * pushForce, ForceMode.Force);
        }
    }
}