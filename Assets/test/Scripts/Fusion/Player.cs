using Fusion;
using System.Globalization;
using System;
using TMPro;
using UnityEngine;
using System.Xml;

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
    [SerializeField] private AnimatorOverrideController[] _animatiaons;

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

    [Networked] private bool _isRunning { get; set; }

    [Networked] private int _selectedAvatarIndex { get; set; }

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

    private void Start()
    {
        SetState(new IdleState(this));
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
        if (_isDead)
        {
            SetState(new DeadState(this));
            return;
        }

        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(_selectedAvatarIndex))
            {
                UpdateAvatar(_selectedAvatarIndex);
            }
        }

        _currentState?.Update();

        _animator.runtimeAnimatorController = _animatiaons[_selectedAvatarIndex];

        if (GetInput(out NetworkInputData data) && CanMove && !_isDead)
        {
            data.direction.Normalize();
            _rb.velocity = data.direction * Speed;

            if (Object.HasStateAuthority)
            {
                _isRunning = data.direction.sqrMagnitude > 0;
                RPC_UpdateAnimation(_isRunning);
            }

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
            SetState(new IdleState(this));
        }
    }

    public override void Spawned()
    {
        if (!HasInputAuthority)
        {
            _playerCamera.gameObject.SetActive(false);
        }

        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasInputAuthority)
        {
            _selectedAvatarIndex = PlayerPrefs.GetInt("SelectedAvatar", 0);
            RPC_SelectAvatar(_selectedAvatarIndex); 
        }

        if (_selectedAvatarIndex >= 0 && _selectedAvatarIndex < _avatarSprites.Length)
        {
            _bodyRenderer.sprite = _avatarSprites[_selectedAvatarIndex];
            if (_selectedAvatarIndex < _animatiaons.Length)
            {
                _animator.runtimeAnimatorController = _animatiaons[_selectedAvatarIndex];
            }
        }

        if (_spawner.IsPlayerHost())
        {
            _hostAvatarIndex = _selectedAvatarIndex;
        }
    }

    private static void OnAvatarChanged(Player player, int previousValue, int newValue)
    {
        player.UpdateAvatar(newValue);
    }

    private void UpdateAvatar(int avatarIndex)
    {
        if (avatarIndex >= 0 && avatarIndex < _avatarSprites.Length)
        {
            _bodyRenderer.sprite = _avatarSprites[avatarIndex];
            if (avatarIndex < _animatiaons.Length)
            {
                _animator.runtimeAnimatorController = _animatiaons[avatarIndex];
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_RequestAvatarSelection()
    {
        int savedAvatar = PlayerPrefs.GetInt("SelectedAvatar", 0);
        RPC_SelectAvatar(savedAvatar);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SelectAvatar(int avatarIndex)
    {
        if (Object.HasStateAuthority)
        {
            _selectedAvatarIndex = avatarIndex;
            RPC_UpdateAvatar(avatarIndex);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateAvatar(int avatarIndex)
    {
        UpdateAvatar(avatarIndex);
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
        return null;
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
        RPC_UpdateHostAnimation(_hostAvatarIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateHostAvatar(int avatarIndex)
    {
        if (_spawner.IsPlayerHost())
        {
            _hostAvatarIndex = avatarIndex;
            _bodyRenderer.sprite = _avatarSprites[avatarIndex];
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateHostAnimation(int avatarIndex)
    {
        _animator.runtimeAnimatorController = _animatiaons[avatarIndex];
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateAnimation(bool isRunning)
    {
        _isRunning = isRunning;
        _animator.SetBool("isRunning", _isRunning);
    }

    public Animator GetAnimator()
    {
        return _animator;
    }

    public void SetAnimation(string animation, bool value)
    {
        _animator.SetBool(animation, value);
    }

    private PlayerStateBase _currentState;

    public bool HasMovementInput()
    {
        return GetInput(out NetworkInputData data) && data.direction.sqrMagnitude > 0;
    }

    public void SetState(PlayerStateBase newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void MovePlayer()
    {
        if (GetInput(out NetworkInputData data))
        {
            _rb.velocity = data.direction * Speed;
        }
    }

    public void DisableMovement()
    {
        Speed = 0;
        _rb.velocity = Vector2.zero;
    }
}