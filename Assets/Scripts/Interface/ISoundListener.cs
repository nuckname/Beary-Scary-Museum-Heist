using UnityEngine;

public interface ISoundListener
{
    void OnSoundHeard(Vector3 soundLocation, Transform soundSource);
}