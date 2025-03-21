using UnityEngine;
using Fusion;

public class SceletonBall : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private int _damage = 2;
    [SerializeField] private float _lifetime = 5f;

    [Networked] private TickTimer Life { get; set; }
    private Rigidbody2D _rb;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Life = TickTimer.CreateFromSeconds(Runner, _lifetime);
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        _rb.velocity = direction * _speed;
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
        Player player = other.gameObject.GetComponent<Player>();
        if (player)
        {
            player.TakeDamage(_damage);
        }

        if (player != null && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}
