using Fusion;
using System.Collections;
using UnityEngine;

public class EnemyDeathManager : NetworkBehaviour
{
    [Networked] private int Health { get; set; } = 3; // Default health
    [SerializeField] private Sprite _deathZombie;
    [SerializeField] private SpriteRenderer _zombieRenderer;

    private Rigidbody2D _rb;
    private bool _isDead = false;

    private KillsCount _playerKillsCount;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _zombieRenderer = transform.Find("Body").GetComponent<SpriteRenderer>();
    }

    // Method to handle getting hit
    public void TakeDamage(int damage, Player player)
    {
        Debug.Log($"Zombie {gameObject.name} took {damage} damage!");

        if (Object.HasStateAuthority)  // Only the State Authority should modify health
        {
            Health -= damage;
            if (Health <= 0)
            {
                Debug.Log("inside if Health");
                _isDead = true;
                RPC_Die(player);
            }
        }
    }

    public bool IsZombieDead()
    {
        return _isDead;
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Die(Player player)
    {
        _isDead = true;

        if (_zombieRenderer != null && _deathZombie != null)
        {
            _zombieRenderer.sprite = _deathZombie;
        }

        KillsCount kc = player.GetComponent<KillsCount>();
        if (kc != null)
        {
            kc.IncrementKills();
        }
        EnemyManager zm = transform.GetComponent<EnemyManager>();
        if (zm != null)
        {
            zm.OnZombieDeath();
        }
        SkeletonManager sm = transform.GetComponent<SkeletonManager>();
        if (sm != null)
        {
            sm.OnSkeletonDeath();
        }
        StartCoroutine(DelayedDespawn());
    }

    private IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(3f);

        if (Object.HasStateAuthority) // Only State Authority should despawn
        {
            Debug.Log($"Despawning zombie {gameObject.name}");
            Runner.Despawn(Object);
        }
    }
}
