using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class ZombieAppearenceManager : NetworkBehaviour
{
    public GameObject zombiePrefab; // Assign your zombie prefab in Inspector
    public int zombieCount = 5;

    public void ZombieSpawned()
    {
        if (Runner.IsServer) // Ensure only the host spawns zombies
        {
            for (int i = 0; i < zombieCount; i++)
            {
                Vector2 randomPos = new Vector2(Random.Range(-15f, 15f), Random.Range(-15f, 15f));
                Runner.Spawn(zombiePrefab, randomPos, Quaternion.identity);
            }
        }
    }
}
