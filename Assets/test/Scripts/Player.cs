using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private Rigidbody2D _rb;
    public float Speed = 2f;
    [SerializeField] private PhysxBall _prefabPhysxBall;

    private Vector3 _forward;
    [Networked] private TickTimer delay { get; set; }
    [Networked] public bool _canMove { get; set; } = false;
    [Networked]
    public bool SpawnedProjectile { get; set; }
    public Material _material;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _forward = transform.up;
        _material = GetComponentInChildren<SpriteRenderer>().material;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data) && _canMove)
        {
            data.direction.Normalize();
            _rb.velocity = data.direction * Speed;

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall,
                      transform.position + _forward,
                      Quaternion.LookRotation(_forward),
                      Object.InputAuthority,
                      (runner, o) =>
                      {
                          o.GetComponent<PhysxBall>().Init(10 * _forward);
                      });
                    SpawnedProjectile = !SpawnedProjectile;
                }
            }
        }
    }

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public void CanMove()
    {
        _canMove = true;
        RPC_AccessCanMove();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_AccessCanMove()
    {
        RPC_CanMove(true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_CanMove(bool canMove)
    {
        Debug.Log($"Client Received CanMove: {canMove}");
        _canMove = canMove;
    }
}
