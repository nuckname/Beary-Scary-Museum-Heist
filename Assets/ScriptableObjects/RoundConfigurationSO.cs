using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoundConfig", menuName = "Game/Round Configuration")]
public class RoundConfigurationSO : ScriptableObject
{
    [Header("Round Settings")]
    public int amountOfArtifactsToCompleteLevel = 3;

    [Header("Guard Spawning")]
    public List<GuardSpawnData> guardsToSpawn;
}

