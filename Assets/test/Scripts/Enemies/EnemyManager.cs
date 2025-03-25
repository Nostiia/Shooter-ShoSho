using Fusion;
using UnityEngine;

public class EnemyManager : NetworkBehaviour
{
    [SerializeField] private float _speed = 0.5f;
    [SerializeField] public int Damage { get; private set; } = 3;
    [Networked] private Vector2 Position { get; set; }

    private bool _isDead = false;
    private Player _targetPlayer;

    public override void Spawned()
    {
        if (Object.HasStateAuthority) 
        {
            Position = transform.position;
            _targetPlayer = FindClosestPlayer();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_isDead) return;

        if (Object.HasStateAuthority)
        {
            UpdateTargetPlayer();
            if (_targetPlayer == null) return;

            Vector2 directionToPlayer = _targetPlayer.transform.position - transform.position;
            if (directionToPlayer.x < 0) 
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else 
            {
                transform.rotation = Quaternion.identity;
            }

            Vector2 targetPosition = _targetPlayer.transform.position;
            Vector2 newPos = Vector2.MoveTowards(Position, targetPosition, _speed * Runner.DeltaTime);

            Position = newPos;
        }

        transform.position = Position;
    }

    private void UpdateTargetPlayer()
    {
        Player newClosestPlayer = FindClosestPlayer();
        if (newClosestPlayer != null && newClosestPlayer != _targetPlayer)
        {
            _targetPlayer = newClosestPlayer;
        }
    }

    private Player FindClosestPlayer()
    {
        Player[] players = FindObjectsOfType<Player>();

        if (players.Length == 0) return null;

        Player closestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (Player player in players)
        {
            if (!player.GetComponent<PlayerHealth>().IsDead())
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPlayer = player;
                }
            }
        }

        return closestPlayer;
    }

    public void OnZombieDeath()
    {
        _isDead = true;
    }

    public bool IsZombieDeath()
    {
        return _isDead;
    }
}
