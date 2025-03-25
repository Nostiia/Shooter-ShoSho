using Fusion;
using UnityEngine;

public class SkeletonManager : NetworkBehaviour
{
    [SerializeField] private float _speed = 1.5f;
    [SerializeField] private float _safeDistance = 5.0f; // Maintain this distance from the player
    [SerializeField] private GameObject _skeletonBallPrefab;
    [SerializeField] private float _attackCooldown = 5f;

    [Networked] private Vector2 Position { get; set; }
    private bool _isDead = false;
    private float _nextAttackTime;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Position = transform.position;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_isDead) return;
        if (!Object.HasStateAuthority) return;

        Player closestPlayer = FindClosestPlayer();
        if (closestPlayer == null) return;

        float distance = Vector2.Distance(transform.position, closestPlayer.transform.position);
        Vector2 moveDirection;

        if (distance > _safeDistance)
        {
            moveDirection = (closestPlayer.transform.position - transform.position).normalized;
        }

        else if (distance < _safeDistance)
        {
            moveDirection = (transform.position - closestPlayer.transform.position).normalized;
        }
        else
        {
            return;
        }

        Vector2 directionToPlayer = closestPlayer.transform.position - transform.position;
        if (Vector2.Dot(transform.right, directionToPlayer) < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        Vector2 newPos = (Vector2)transform.position + moveDirection * _speed * Runner.DeltaTime;
        Position = newPos;

        if (Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + _attackCooldown;
            Player target = FindClosestPlayer();
            if (target != null)
            {
                Rpc_SpawnEnergyBall(transform.position, target.transform.position);
            }
        }

        transform.position = Position;
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_SpawnEnergyBall(Vector2 spawnPosition, Vector2 targetPosition)
    {
        float playerDirection = targetPosition.x - spawnPosition.x;
        Vector2 spawnOffset = playerDirection < 0 ? new Vector2(-1, 0) : new Vector2(1, 0); 

        NetworkObject networkObject = Runner.Spawn(_skeletonBallPrefab, spawnPosition + spawnOffset, Quaternion.identity);
        networkObject.gameObject.GetComponent<SceletonBall>().Initialize(targetPosition);
    }

    public void OnSkeletonDeath()
    {
        if (Object.HasStateAuthority)
        {
            _isDead = true;
        }
    }

    public bool IsSkeletonDead()
    {
        return _isDead;
    }
}
