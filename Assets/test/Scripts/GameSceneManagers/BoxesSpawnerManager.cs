using Fusion;
using UnityEngine;

public class BoxesSpawnerManager : NetworkBehaviour
{
    [SerializeField] private GameObject _boxPrefab;
    [SerializeField] private int _boxCount = 3;
    [SerializeField] private float _spawnBoxesBorder = 10f;

    public void BoxSpawned()
    {
        if (Runner.IsServer) // Ensure only the host spawns zombies
        {
            for (int i = 0; i < _boxCount; i++)
            {
                Vector2 randomPos = new Vector2(Random.Range(-_spawnBoxesBorder, _spawnBoxesBorder), Random.Range(-_spawnBoxesBorder, _spawnBoxesBorder));
                Runner.Spawn(_boxPrefab, randomPos, Quaternion.identity);
            }
        }
    }
}
