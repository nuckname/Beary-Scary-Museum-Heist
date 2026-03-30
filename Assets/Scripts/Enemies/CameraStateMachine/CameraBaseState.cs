using UnityEngine;

public abstract class CameraBaseState
{
    public abstract void EnterState(SecurityCameraController camera);
    public abstract void UpdateState(SecurityCameraController camera);
}