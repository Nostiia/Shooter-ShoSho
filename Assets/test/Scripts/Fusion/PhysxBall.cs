using UnityEngine;
using Fusion;

public class PhysxBall : NetworkBehaviour
{
    [Networked] private TickTimer Life { get; set; }
    public int Damage { get; set; } = 1;
    private Rigidbody2D _rb;
    private Collider2D _collider;

    private Player _player;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        foreach (PhysxBall projectile in FindObjectsOfType<PhysxBall>())
        {
            if (projectile != this)
            {
                Physics2D.IgnoreCollision(_collider, projectile.GetComponent<Collider2D>());
            }
        }
        StartCoroutine(IgnoreCollisionsAfterSpawn());
    }

    private System.Collections.IEnumerator IgnoreCollisionsAfterSpawn()
    {
        yield return new WaitForSeconds(0.1f);  // Small delay to ensure all objects are initialized

        foreach (PhysxBall projectile in FindObjectsOfType<PhysxBall>())
        {
            if (projectile != this)
            {
                Physics2D.IgnoreCollision(_collider, projectile._collider);
            }
        }
    }

    public void Init(Vector3 forward, Player player)
    {
        _player = player;
        Life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        _rb.velocity = forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (Life.Expired(Runner) && Object.HasStateAuthority)
        {
            Debug.Log("Despawning projectile...");
            Runner.Despawn(Object);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        ZombieDeathManager zombie = collision.gameObject.GetComponent<ZombieDeathManager>();

        if (zombie != null && !zombie.IsZombieDead())
        {
            Debug.Log("Hit a zombie: " + collision.gameObject.name);

            if (Object != null && Object.HasStateAuthority) 
            {
                zombie.TakeDamage(Damage, _player);
            }
            else if (zombie.Object != null) 
            {
                RPC_RequestDamage(zombie.Object, Damage);
            }
        }

         //Prevent null reference before despawning
        if (Object != null && Object.HasStateAuthority)
        {
            Debug.Log("Despawning projectile...");
            Runner.Despawn(Object);
        }
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestDamage(NetworkObject zombieObject, int damage)
    {
        if (zombieObject != null)
        {
            ZombieDeathManager zombie = zombieObject.GetComponent<ZombieDeathManager>();
            if (zombie != null)
            {
                zombie.TakeDamage(damage, _player);
            }
        }
    }
}
