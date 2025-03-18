using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class AmmoPlusProperties : NetworkBehaviour
{
    [SerializeField] private int _ammoPlus = 5;
    [Networked] private Vector2 Position { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Position = transform.position;
            Rpc_SpawnAmmo();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.gameObject.GetComponent<Player>();
        AmmoCount ammoCount = player.transform.GetComponent<AmmoCount>();
        ammoCount.IncrementAmmo(_ammoPlus);

        if (Object != null && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_SpawnAmmo()
    {
        transform.position = Position;
    }
}
