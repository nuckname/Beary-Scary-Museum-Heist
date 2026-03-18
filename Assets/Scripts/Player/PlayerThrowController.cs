using UnityEngine;

public class PlayerThrowController : MonoBehaviour
{
    [SerializeField] private float minThrowForce = 2f;
    [SerializeField] private float maxThrowForce = 15f;
    [SerializeField] private float chargeRate = 15f;

    private float currentThrowForce;
    private bool isCharging;

    private PlayerGrabController grabController;

    private void Awake()
    {
        grabController = GetComponent<PlayerGrabController>();
        currentThrowForce = minThrowForce;
    }

    private void Update()
    {
        if (grabController.PickedUpObject == null)
        {
            isCharging = false;
            currentThrowForce = minThrowForce;
            return;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            isCharging = true;
            currentThrowForce = minThrowForce;
        }

        if (isCharging && Input.GetKey(KeyCode.G))
        {
            currentThrowForce += chargeRate * Time.deltaTime;
            currentThrowForce = Mathf.Clamp(currentThrowForce, minThrowForce, maxThrowForce);
        }

        if (isCharging && Input.GetKeyUp(KeyCode.G))
        {
            ThrowObject();
        }
    }

    private void ThrowObject()
    {
        GameObject objectToThrow = grabController.PickedUpObject;
        
        grabController.ReleaseObject();

        Rigidbody boxRb = objectToThrow.GetComponent<Rigidbody>();
        if (boxRb != null)
        {
            boxRb.isKinematic = false;
            boxRb.useGravity = true;

            Vector3 throwDirection = transform.forward + (Vector3.up * 0.5f);
            boxRb.linearVelocity = throwDirection.normalized * currentThrowForce;
        }

        isCharging = false;
        currentThrowForce = minThrowForce;
    }
}