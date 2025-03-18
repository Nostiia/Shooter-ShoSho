using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class MedKit : NetworkBehaviour
{
    [SerializeField] private int _helthPlus = 5;
    [Networked] private Vector2 Position { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority) 
        {
            Position = transform.position;
            Rpc_SpawnMedKit();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.gameObject.GetComponent<Player>();
        HPCount hpCount = player.transform.GetComponent<HPCount>();
        hpCount.IncrementHP(_helthPlus);

        if (Object != null && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_SpawnMedKit()
    {
        transform.position = Position;
    }
}
