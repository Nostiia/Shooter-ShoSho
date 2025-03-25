using Fusion;
using UnityEngine;

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
        AmmoCount ammoCount = other.gameObject.GetComponent<AmmoCount>();
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
