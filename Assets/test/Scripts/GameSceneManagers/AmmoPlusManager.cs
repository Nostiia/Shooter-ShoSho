using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class AmmoPlusManager : NetworkBehaviour
{
    [SerializeField] private GameObject _ammoPrefab;
    [SerializeField] private int _ammoCount = 3;

    public void AmmoPlusSpawned()
    {
        if (Runner.IsServer) // Ensure only the host spawns zombies
        {
            for (int i = 0; i < _ammoCount; i++)
            {
                Vector2 randomPos = new Vector2(Random.Range(-15f, 15f), Random.Range(-15f, 15f));
                Runner.Spawn(_ammoPrefab, randomPos, Quaternion.identity);
            }
        }
    }
}
