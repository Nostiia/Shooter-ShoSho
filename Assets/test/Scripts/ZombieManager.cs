using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieManager : NetworkBehaviour
{
    [SerializeField] private float _speed = 1f;
    [Networked] private Vector2 Position { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority) // Ensures only one instance controls movement
        {
            Position = transform.position;
        }
    }
    public override void FixedUpdateNetwork()
    {
 
        if (Object.HasStateAuthority)
        {
            Player closestPlayer = FindClosestPlayer();
            if (closestPlayer == null) return;

            Vector2 targetPosition = closestPlayer.transform.position;
            Vector2 newPos = Vector2.MoveTowards(Position, targetPosition, _speed * Runner.DeltaTime);

            Position = newPos;
            Rpc_MoveZombie(newPos);

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
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_MoveZombie(Vector2 newPosition)
    {
        Position = newPosition;
        transform.position = newPosition;
    }
}
