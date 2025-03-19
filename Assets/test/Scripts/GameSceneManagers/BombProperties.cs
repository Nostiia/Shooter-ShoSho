using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Collections.Unicode;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BombProperties : NetworkBehaviour
{
    [SerializeField] private float _deathRadius = 10f;
    [Networked] private Vector2 Position { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Position = transform.position;
            Rpc_SpawnBomb();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.gameObject.GetComponent<Player>();
        KillsCount kc = player.transform.GetComponent<KillsCount>();

        if (other.GetComponent<Player>() && Object.HasStateAuthority)
        {
            Rpc_Explode(kc, player);
            Runner.Despawn(Object);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_SpawnBomb()
    {
        transform.position = Position;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_Explode(KillsCount killsCount, Player player)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _deathRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent(out NetworkObject netObj))
            {
                if (collider.GetComponent<EnemyDeathManager>())
                {
                    if (netObj.HasStateAuthority)
                    {
                        collider.GetComponent<EnemyDeathManager>().RPC_Die(player);
                    }
                }
            }
        }
    }
}
