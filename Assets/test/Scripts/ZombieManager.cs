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
            Vector2 newPos = Vector2.MoveTowards(Position, Vector2.zero, _speed * Runner.DeltaTime);
            Position = newPos;
            Rpc_MoveZombie(newPos);
        }

        transform.position = Position;

    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_MoveZombie(Vector2 newPosition)
    {
        Position = newPosition;
        transform.position = newPosition;
    }
}
