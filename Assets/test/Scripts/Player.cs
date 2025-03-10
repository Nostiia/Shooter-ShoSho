using Fusion;
using System.Globalization;
using System;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private Rigidbody2D _rb;
    public float Speed = 2f;
    [SerializeField] private PhysxBall _prefabPhysxBall;

    private Vector3 _forward;
    [Networked] private TickTimer _delay { get; set; }
    [Networked] public bool _canMove { get; set; } = false;
    [Networked] public bool SpawnedProjectile { get; set; }
    private SpriteRenderer _bodyRenderer;
    [SerializeField] private Sprite[] _avatarSprites;
    private int _selectedAvatarIndex;
    private ChangeDetector _changeDetector;
    private int _hostAvatarIndex = 0;
    [SerializeField] private BasicSpawner _spawner;

    private WeaponManager _weaponManager;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _forward = transform.up;
        _bodyRenderer = transform.Find("Body").GetComponent<SpriteRenderer>();
        _spawner = FindObjectOfType<BasicSpawner>();
        _weaponManager = GetComponent<WeaponManager>();
    }

    public override void FixedUpdateNetwork()
    {
        int _weaponIndex = _weaponManager.WeaponIndex();

        if (GetInput(out NetworkInputData data) && _canMove)
        {
            data.direction.Normalize();
            _rb.velocity = data.direction * Speed;

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if (HasStateAuthority && _delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    _delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall,
                      transform.position + _forward,
                      Quaternion.LookRotation(_forward),
                      Object.InputAuthority,
                      (runner, o) =>
                      {
                          o.GetComponent<PhysxBall>().Init(10 * _forward);
                      });

                    if (_weaponIndex == 1)
                    {
                        Vector3 directionUp = Quaternion.Euler(0, 0, 10) * _forward;  // Rotate +20 degrees
                        Vector3 directionDown = Quaternion.Euler(0, 0, -10) * _forward; // Rotate -20 degrees

                        // Spawn the upper projectile
                        Runner.Spawn(_prefabPhysxBall,
                          transform.position + directionUp,
                          Quaternion.LookRotation(directionUp, Vector3.back),
                          Object.InputAuthority,
                          (runner, o) =>
                          {
                              o.GetComponent<PhysxBall>().Init(10 * directionUp);
                          });

                        // Spawn the lower projectile
                        Runner.Spawn(_prefabPhysxBall,
                          transform.position + directionDown,
                          Quaternion.LookRotation(directionDown, Vector3.back),
                          Object.InputAuthority,
                          (runner, o) =>
                          {
                              o.GetComponent<PhysxBall>().Init(10 * directionDown);
                          });
                    }

                    SpawnedProjectile = !SpawnedProjectile;
                }
            }
        }
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        _selectedAvatarIndex = PlayerPrefs.GetInt("SelectedAvatar", 0);

        if (_selectedAvatarIndex >= 0 && _selectedAvatarIndex < _avatarSprites.Length)
        {
            _bodyRenderer.sprite = _avatarSprites[_selectedAvatarIndex];
        }
        if (_spawner.IsPlayerHost())
        {
            _hostAvatarIndex = _selectedAvatarIndex;
        }
    }

    public void CanMove()
    {
        _canMove = true;
        RPC_AccessCanMove();
    }

    public void SaveHostIndex( int avatarindex)
    {
        if (_spawner.IsPlayerHost())
        {
            _hostAvatarIndex = avatarindex;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_AccessCanMove()
    {
        RPC_CanMove(true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_CanMove(bool canMove)
    {
        _canMove = canMove;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_HostSelectAvatar()
    {
        RPC_UpdateHostAvatar(_hostAvatarIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_UpdateHostAvatar(int avatarIndex)
    {
        // Ensure the avatarIndex is within valid range
        if (avatarIndex >= 0 && avatarIndex < _avatarSprites.Length)
        {
            _bodyRenderer.sprite = _avatarSprites[avatarIndex];
        }
    }    
}
