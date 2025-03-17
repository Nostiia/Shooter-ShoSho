using System.Collections;
using UnityEngine;
using Fusion;

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

        // If too far, move towards the player
        if (distance > _safeDistance)
        {
            moveDirection = (closestPlayer.transform.position - transform.position).normalized;
        }
        // If too close, move away from the player
        else if (distance < _safeDistance)
        {
            moveDirection = (transform.position - closestPlayer.transform.position).normalized;
        }
        else
        {
            return;
        }

        // Move the skeleton
        Vector2 newPos = (Vector2)transform.position + moveDirection * _speed * Runner.DeltaTime;
        Position = newPos;
        Rpc_MoveSkeleton(newPos);

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
            if (player.IsPlayerAlive())
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
    private void Rpc_MoveSkeleton(Vector2 newPosition)
    {
        Position = newPosition;
        transform.position = newPosition;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_SpawnEnergyBall(Vector2 spawnPosition, Vector2 targetPosition)
    {
        GameObject ball = Instantiate(_skeletonBallPrefab, spawnPosition, Quaternion.identity);
        ball.GetComponent<SceletonBall>().Initialize(targetPosition);
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
