using UnityEngine;
using Fusion;

public class PhysxBall : NetworkBehaviour
{
    [Networked] private TickTimer Life { get; set; }
    public int Damage { get; set; } = 1;
    private Rigidbody2D _rb;
    private Collider2D _collider;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>(); 
    }

    public void Init(Vector3 forward)
    {
        Life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        _rb.velocity = forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (Life.Expired(Runner) && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        ZombieDeathManager zombie = collision.gameObject.GetComponent<ZombieDeathManager>();

        if (zombie != null)
        {
            Debug.Log("Hit a zombie: " + collision.gameObject.name);

            if (Object.HasStateAuthority)
            {
                zombie.TakeDamage(Damage);
            }
            else
            {
                // Send an RPC to the server to apply damage
                RPC_RequestDamage(zombie.Object, Damage);
            }
        }

        if (Object.HasStateAuthority)
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
                zombie.TakeDamage(damage);
            }
        }
    }
}
