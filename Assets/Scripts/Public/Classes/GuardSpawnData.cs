using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GuardSpawnData
{
    public GameObject guardPrefab;
    
    public int pathHolderIndex;

    [Header("Custom Override Settings")]
    public float guardPatrolSpeed = 3f;
    public float guardChaseSpeed = 5f;
    public int amountOfTimesTheGuardTurns = 2;
    public float FLOATturnVisionRadiusReductionPercentage = 0.2f;
    public bool alwaysTurnRight = false;
}