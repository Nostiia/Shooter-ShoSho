using Fusion;
using UnityEngine;

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
            Rpc_Explode(player);
            Runner.Despawn(Object);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_SpawnBomb()
    {
        transform.position = Position;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_Explode(Player player)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _deathRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent(out NetworkObject netObj)
                 && collider.GetComponent<EnemyDeathManager>()
                 && netObj.HasStateAuthority)
            {
                collider.GetComponent<EnemyDeathManager>().RPC_Die(player);
            }           
        }
    }
}
