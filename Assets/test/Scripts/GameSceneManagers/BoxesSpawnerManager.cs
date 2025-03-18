using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class BoxesSpawnerManager : NetworkBehaviour
{
    [SerializeField] private GameObject _boxPrefab;
    [SerializeField] private int _boxCount = 3;

    public void BoxSpawned()
    {
        if (Runner.IsServer) // Ensure only the host spawns zombies
        {
            for (int i = 0; i < _boxCount; i++)
            {
                Vector2 randomPos = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                Runner.Spawn(_boxPrefab, randomPos, Quaternion.identity);
            }
        }
    }
}
