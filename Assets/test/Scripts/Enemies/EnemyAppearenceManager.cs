using Fusion;
using UnityEngine;

public class EnemyAppearenceManager : NetworkBehaviour
{
    [SerializeField] private GameObject _zombiePrefab;
    [SerializeField] private int _zombieCount = 5;
    [SerializeField] private float _spawnBorder = 20f;
    public void ZombieSpawned()
    {
        if (Runner.IsServer)
        {
            for (int i = 0; i < _zombieCount; i++)
            {
                Vector2 randomPos = new Vector2(Random.Range(-_spawnBorder, _spawnBorder), Random.Range(-_spawnBorder,_spawnBorder));
                Runner.Spawn(_zombiePrefab, randomPos, Quaternion.identity);
            }
        }
    }
}
