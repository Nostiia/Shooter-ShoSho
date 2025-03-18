using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class MedKitManager : NetworkBehaviour
{
    [SerializeField] private GameObject _medkitPrefab;
    [SerializeField] private int _medkitCount = 3;

    public void MedkitSpawned()
    {
        if (Runner.IsServer) // Ensure only the host spawns zombies
        {
            for (int i = 0; i < _medkitCount; i++)
            {
                Vector2 randomPos = new Vector2(Random.Range(-15f, 15f), Random.Range(-15f, 15f));
                Runner.Spawn(_medkitPrefab, randomPos, Quaternion.identity);
            }
        }
    }
}
