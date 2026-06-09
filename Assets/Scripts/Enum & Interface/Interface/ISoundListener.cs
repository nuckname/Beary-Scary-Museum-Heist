using UnityEngine;

public interface ISoundListener
{
    void OnSoundHeard(Vector3 originPosition, Transform sourceTransform, NoiseType noiseType);
}