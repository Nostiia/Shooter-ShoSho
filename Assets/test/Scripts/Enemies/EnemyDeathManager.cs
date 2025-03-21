using Fusion;
using System.Collections;
using UnityEngine;

public class EnemyDeathManager : NetworkBehaviour
{
    [Networked] public int Health { get; set; } = 3; 

    private Rigidbody2D _rb;
    private bool _isDead = false;

    private KillsCount _playerKillsCount;

    private Animator _animator;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = transform.Find("Body").GetComponent<Animator>();
    }

    public void TakeDamage(int damage, Player player)
    {
        if (Object.HasStateAuthority) 
        {
            Health -= damage;
            _animator.SetBool("isHitted", true);
            RPC_Hitted();
            StartCoroutine(ResetHitAnimation());
            player.transform.GetComponent<KillsCount>().AddDamage(damage);
            if (Health <= 0)
            {
                _isDead = true;
                _animator.SetBool("isDied", _isDead);
                RPC_Die(player);
            }
        }
    }

    private IEnumerator ResetHitAnimation()
    {
        yield return new WaitForSeconds(1f);
        _animator.SetBool("isHitted", false);
    }

    public bool IsZombieDead()
    {
        return _isDead;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Hitted()
    {
        _animator.SetBool("isHitted", true);
        StartCoroutine(ResetHitAnimation());
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Die(Player player)
    {
        _isDead = true;
        _animator.SetBool("isDied", _isDead);
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

        if (Object.HasStateAuthority) 
        {
            Runner.Despawn(Object);
        }
    }
}
