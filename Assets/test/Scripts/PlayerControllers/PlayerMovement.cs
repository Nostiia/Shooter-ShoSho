using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody2D _rb;
    [SerializeField] private float Speed = 2f;
    private Player _player;

    [Networked] private bool _isRunning { get; set; }
    [Networked] public bool CanMove { get; private set; } = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _player = GetComponent<Player>();
    }

    public void HandleMovement()
    {
        if (_player.GetInput(out NetworkInputData data) && CanMove)
        {
            data.direction.Normalize();
            _rb.velocity = data.direction * Speed;

            if (Object.HasStateAuthority)
            {
                _isRunning = data.direction.sqrMagnitude > 0;
                RPC_UpdateAnimation(_isRunning);
            }
        }
        else
        {
            _player.SetState(new IdleState(_player));
        }
    }

    public void DisableMovement()
    {
        Speed = 0;
        _rb.velocity = Vector2.zero;
    }

    public void MovePlayer()
    {
        if (GetInput(out NetworkInputData data))
        {
            _rb.velocity = data.direction * Speed;
        }
    }

    public bool HasMovementInput()
    {
        return GetInput(out NetworkInputData data) && data.direction.sqrMagnitude > 0;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateAnimation(bool isRunning)
    {
        _isRunning = isRunning;
        _player.SetAnimation("isRunning", _isRunning);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_AccessCanMove()
    {
        RPC_CanMove(true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_CanMove(bool canMove)
    {
        CanMove = canMove;
    }
}
