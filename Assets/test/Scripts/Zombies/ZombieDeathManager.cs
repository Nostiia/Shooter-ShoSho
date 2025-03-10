using Fusion;
using System.Collections;
using UnityEngine;

public class ZombieDeathManager : NetworkBehaviour
{
    [Networked] private int Health { get; set; } = 3; // Default health
    [SerializeField] private Sprite _deathZombie;
    [SerializeField] private SpriteRenderer _zombieRenderer;

    private Rigidbody2D _rb;
    private bool _isDead = false;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _zombieRenderer = transform.Find("BodyZombie").GetComponent<SpriteRenderer>();
    }

    // Method to handle getting hit
    public void TakeDamage(int damage)
    {
        Debug.Log($"Zombie {gameObject.name} took {damage} damage!");

        if (Object.HasStateAuthority)  // Only the State Authority should modify health
        {
            Health -= damage;
            if (Health <= 0)
            {
                RPC_Die();
            }
        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Die()
    {
        if (_isDead) return; 
        _isDead = true;

        if (_zombieRenderer != null && _deathZombie != null)
        {
            _zombieRenderer.sprite = _deathZombie;
        }
        ZombieManager zm = transform.GetComponent<ZombieManager>();
        zm.OnZombieDeath();
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
