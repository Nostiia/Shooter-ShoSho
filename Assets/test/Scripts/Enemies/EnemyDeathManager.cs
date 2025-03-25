using Fusion;
using System.Collections;
using UnityEngine;

public class EnemyDeathManager : NetworkBehaviour
{
    [Networked] public int Health { get; set; } = 3; 
    private bool _isDead = false;
    [SerializeField] private Animator _animator;

    private int HittedAnimationHash = Animator.StringToHash("isHitted");
    private int DiedAnimationHash = Animator.StringToHash("isDied");

    public void TakeDamage(int damage, Player player)
    {
        if (Object.HasStateAuthority) 
        {
            Health -= damage;
            _animator.SetBool(HittedAnimationHash, true);
            RPC_Hitted();
            StartCoroutine(ResetHitAnimation());
            player.transform.GetComponent<KillsCount>().AddDamage(damage);
            if (Health <= 0)
            {
                _isDead = true;
                _animator.SetBool(DiedAnimationHash, _isDead);
                RPC_Die(player);
            }
        }
    }

    private IEnumerator ResetHitAnimation()
    {
        yield return new WaitForSeconds(1f);
        _animator.SetBool(HittedAnimationHash, false);
    }

    public bool IsZombieDead()
    {
        return _isDead;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Hitted()
    {
        _animator.SetBool(HittedAnimationHash, true);
        StartCoroutine(ResetHitAnimation());
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Die(Player player)
    {
        _isDead = true;
        _animator.SetBool(DiedAnimationHash, _isDead);
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
