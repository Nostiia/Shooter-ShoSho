using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class ZombieAppearenceManager : NetworkBehaviour
{
    [SerializeField] private GameObject _zombiePrefab;
    [SerializeField] private int _zombieCount = 5;

    public void ZombieSpawned()
    {
        if (Runner.IsServer) // Ensure only the host spawns zombies
        {
            for (int i = 0; i < _zombieCount; i++)
            {
                Vector2 randomPos = new Vector2(Random.Range(-15f, 15f), Random.Range(-15f, 15f));
                Runner.Spawn(_zombiePrefab, randomPos, Quaternion.identity);
            }
        }
    }
}
