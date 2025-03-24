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
    private float _weaponLifetime = 6;

    private int[] _damageBasedOnIndex = { 3, 2, 1 };
    private float[] _lifeTimeBasedOnIndex = { 0.2f, 0.5f, 2.0f };

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
    }

    public void Init(Vector3 forward, Player player)
    {
        _player = player;
        SetWeaponProperties();
        Life = TickTimer.CreateFromSeconds(Runner, _weaponLifetime);
        _rb.velocity = forward;
    }

    private void SetWeaponProperties()
    {
        _weaponIndex = _player.transform.GetComponent<PlayerShooting>().GetWeaponIndex();
        switch (_weaponIndex)
        {
            case 0:
                Damage = _damageBasedOnIndex[_weaponIndex];
                _weaponLifetime = _lifeTimeBasedOnIndex[_weaponIndex];
                break;
            case 1:
                Damage = _damageBasedOnIndex[_weaponIndex];
                _weaponLifetime = _lifeTimeBasedOnIndex[_weaponIndex];
                break;
            case 2:
                Damage = _damageBasedOnIndex[_weaponIndex];
                _weaponLifetime = _lifeTimeBasedOnIndex[_weaponIndex];
                break;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Life.Expired(Runner) && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyDeathManager zombie = other.gameObject.GetComponent<EnemyDeathManager>();
        if (zombie != null && !zombie.IsZombieDead())
        {
            if (Object != null && Object.HasStateAuthority) 
            {
                zombie.TakeDamage(Damage, _player);
            }
            else if (zombie.Object != null) 
            {
                RPC_RequestDamage(zombie.Object, Damage);
            }

            if (Object != null && Object.HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
        }
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestDamage(NetworkObject zombieObject, int damage)
    {
        if (zombieObject != null)
        {
            EnemyDeathManager zombie = zombieObject.GetComponent<EnemyDeathManager>();
            if (zombie != null)
            {
                zombie.TakeDamage(damage, _player);
            }
        }
    }
}
