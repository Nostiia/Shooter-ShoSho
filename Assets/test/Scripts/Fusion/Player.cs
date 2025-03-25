using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public float Speed = 2f;
    [SerializeField] private PhysxBall _prefabPhysxBall;
    [SerializeField] private SpriteRenderer _bodyRenderer;
    [SerializeField] private Sprite[] _avatarSprites;
    [SerializeField] private AnimatorOverrideController[] _animatiaons;
    private ChangeDetector _changeDetector;
    private int _hostAvatarIndex = 0;
    [SerializeField] private BasicSpawner _spawner;
    private bool _isDead = false;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Animator _animator;
    [Networked] private int _selectedAvatarIndex { get; set; }
    [Networked] public int Id { get; private set; } = 0;
    private PlayerStateBase _currentState;
    private PlayerShooting _playerShooting;
    private PlayerMovement _playerMovement;
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _spawner = FindObjectOfType<BasicSpawner>();
        _playerShooting = GetComponent<PlayerShooting>();
        _playerMovement = GetComponent<PlayerMovement>(); 
        _playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        SetState(new IdleState(this));
    }

    public void SetID(int id)
    {
        Id = id;
    }

    public override void FixedUpdateNetwork()
    {
        _isDead = _playerHealth.IsDead();
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

        if (!_isDead && _playerMovement.CanMove)
        {
            _playerMovement.HandleMovement();

            _playerShooting.HandleShooting();
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

            Player anotherPlayer = _playerHealth.FindAnotherAlivePlayer().transform.GetComponent<Player>();
            if (anotherPlayer != null)
            {
                _playerCamera.gameObject.SetActive(isAlive);
                anotherPlayer._playerCamera.gameObject.SetActive(true);
            }
        }
    }

    public void SaveHostIndex(int avatarindex)
    {
        if (_spawner.IsPlayerHost())
        {
            _hostAvatarIndex = avatarindex;
        }
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

    public void SetAnimation(string animation, bool value)
    {
        _animator.SetBool(animation, value);
    }

    public void SetState(PlayerStateBase newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
}