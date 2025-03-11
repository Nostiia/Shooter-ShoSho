using UnityEngine;
using Fusion;

public class PhysxBall : NetworkBehaviour
{
    [Networked] private TickTimer Life { get; set; }
    public int Damage { get; set; } = 0;
    private Rigidbody2D _rb;
    private Collider2D _collider;

    private Player _player;
    private int _weaponIndex = -1;
    private float _weaponLifetime = 6f;

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
        SetWeaponProperties();
        Debug.Log($"{_weaponLifetime}");
        Life = TickTimer.CreateFromSeconds(Runner, _weaponLifetime);
        _rb.velocity = forward;
    }

    private void SetWeaponProperties()
    {
        _weaponIndex = _player.GetPlayersWeaponIndex();
        switch (_weaponIndex)
        {
            case 0:
                Damage = 3;
                _weaponLifetime = 0.2f;
                break;
            case 1:
                Damage = 2;
                _weaponLifetime = 0.5f;
                break;
            case 2:
                Damage = 1;
                _weaponLifetime = 2.0f;
                break;
        }
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
