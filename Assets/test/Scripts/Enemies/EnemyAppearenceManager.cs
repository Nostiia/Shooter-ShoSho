using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class EnemyAppearenceManager : NetworkBehaviour
{
    [SerializeField] private GameObject _zombiePrefab;
    [SerializeField] private int _zombieCount = 5;

    public void ZombieSpawned()
    {
        if (Runner.IsServer)
        {
            for (int i = 0; i < _zombieCount; i++)
            {
                Vector2 randomPos = new Vector2(Random.Range(-20f, 20f), Random.Range(-20f, 20f));
                Runner.Spawn(_zombiePrefab, randomPos, Quaternion.identity);
            }
        }
    }
}
