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
    private Vector3 _shootDirection;
    [Networked] private TickTimer _delay { get; set; }
    [Networked] public bool CanMove { get; set; } = false;
    [Networked] public bool SpawnedProjectile { get; set; }

    private SpriteRenderer _bodyRenderer;
    private SpriteRenderer _weaponRenderer;

    [SerializeField] private Sprite[] _avatarSprites;
    private int _selectedAvatarIndex;

    private ChangeDetector _changeDetector;
    private int _hostAvatarIndex = 0;

    [SerializeField] private BasicSpawner _spawner;

    private WeaponManager _weaponManager;
    private AmmoCount _ammoCounter;
    private HPCount _hpCounter;

    private bool _isDead = false;
    private int _weaponIndex = -1;

    [SerializeField] private Camera _playerCamera;

    private Animator _animator;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _forward = transform.up;
        _bodyRenderer = transform.Find("Body").GetComponent<SpriteRenderer>();
        _animator = transform.Find("Body").GetComponent<Animator>();
        _weaponRenderer = transform.Find("Weapon").GetComponent<SpriteRenderer>();
        _spawner = FindObjectOfType<BasicSpawner>();
        _weaponManager = transform.GetComponent<WeaponManager>();
        _ammoCounter = transform.GetComponent<AmmoCount>();
        _hpCounter = GetComponent<HPCount>();
    }

    [Networked] private int _id { get; set; } = 0;

    public void SetID(int id)
    {
        _id = id;
    }

    public int GetId()
    {
        return _id;
    }

    public void TakeDamage(int damage)
    {
        _hpCounter.DecrementHP(damage);
    }

    public override void FixedUpdateNetwork()
    {
        _isDead = _hpCounter.IsPlayerDied();


        if (GetInput(out NetworkInputData data) && CanMove && !_isDead)
        {
            data.direction.Normalize();
            _rb.velocity = data.direction * Speed;

            bool isMoving = data.direction.sqrMagnitude > 0;
            _animator.SetBool("isRunning", isMoving);

            data.shootDirection.Normalize();
            if (data.shootDirection.sqrMagnitude > 0)
            {
                _shootDirection = data.shootDirection;

                float angle = Mathf.Atan2(_shootDirection.y, _shootDirection.x) * Mathf.Rad2Deg;
                float angleY = 0;
                if (Vector2.Dot(transform.right, _shootDirection) < 0)
                {
                    _bodyRenderer.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    angleY = 180;
                    angle = -angle;
                }
                else
                {
                    _bodyRenderer.transform.localRotation = Quaternion.identity;
                }
                _weaponRenderer.transform.rotation = Quaternion.Euler(angleY, 0, angle);
            }

            if (HasStateAuthority && _delay.ExpiredOrNotRunning(Runner) && _ammoCounter.GetCurrentAmmo() > 0)
            {
                _weaponIndex = _weaponManager.GetWeaponIndex();
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1) || data.buttons.IsSet(NetworkInputData.SHOOT))
                {
                    _delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall,
                      transform.position + _shootDirection,
                      Quaternion.LookRotation(_shootDirection),
                      Object.InputAuthority,
                      (runner, o) =>
                      {
                          o.GetComponent<PhysxBall>().Init(10 * _shootDirection, this);
                      });

                    if (_weaponManager.GetWeaponIndex() == 1)
                    {
                        Vector3 rotatedForwardLeft = Quaternion.Euler(0, 0, 10) * _shootDirection;
                        Vector3 rotatedForwardRight = Quaternion.Euler(0, 0, -10) * _shootDirection;

                        Runner.Spawn(_prefabPhysxBall,
                          transform.position + rotatedForwardLeft,
                          Quaternion.LookRotation(rotatedForwardLeft),
                          Object.InputAuthority,
                          (runner, o) =>
                          {
                              o.GetComponent<PhysxBall>().Init(10 * rotatedForwardLeft, this);
                          });
                        Runner.Spawn(_prefabPhysxBall,
                          transform.position + rotatedForwardRight,
                          Quaternion.LookRotation(rotatedForwardRight),
                          Object.InputAuthority,
                          (runner, o) =>
                          {
                              o.GetComponent<PhysxBall>().Init(10 * rotatedForwardRight, this);
                          });
                    }

                    if (_ammoCounter != null)
                    {
                        _ammoCounter.DecrementAmmo();
                    }
                    SpawnedProjectile = !SpawnedProjectile;
                }
            }
        }
        else
        {
            _animator.SetBool("isRunning", false);
        }
    }

    public override void Spawned()
    {
        if (!HasInputAuthority)
        {
            _playerCamera.gameObject.SetActive(false);
        }

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

    public void SwitchCameras(bool isAlive)
    {
        if (HasInputAuthority)
        {

            Player anotherPlayer = FindAnotherAlivePlayer();
            if (anotherPlayer != null)
            {
                _playerCamera.gameObject.SetActive(isAlive);
                anotherPlayer._playerCamera.gameObject.SetActive(true);
            }
        }
    }

    private Player FindAnotherAlivePlayer()
    {
        foreach (var player in FindObjectsOfType<Player>())
        {
            if (player != this && !player._isDead)
            {
                return player;
            }
        }
        return null; // No other alive player found
    }

    public void CouldMove()
    {
        CanMove = true;
        RPC_AccessCanMove();
    }

    public void SaveHostIndex(int avatarindex)
    {
        if (_spawner.IsPlayerHost())
        {
            _hostAvatarIndex = avatarindex;
        }
    }

    public bool IsPlayerAlive()
    {
        return !_isDead;
    }

    public int GetPlayersWeaponIndex()
    {
        return _weaponIndex;
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