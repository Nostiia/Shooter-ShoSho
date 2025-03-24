using Fusion;
using TMPro;
using UnityEngine;

public class HPCount : NetworkBehaviour
{
    [Networked] public int HP { get; set; } = 20;
    private TMP_Text _hpText;
    private int _hpMax;

    private Rigidbody2D _rb;
    private bool _isDied = false;
    [SerializeField] private Animator _animator;

    private const string HPText = "HP";
    private int DiedAnimationHash = Animator.StringToHash("isDied");

    private void Start()
    {
        _hpText = GameObject.Find(HPText)?.GetComponent<TMP_Text>();
        _hpMax = HP;
        _rb = GetComponent<Rigidbody2D>();

        if (_hpText == null)
        {
            Debug.LogError("HPText UI not found in the scene!");
        }

        UpdateHPUI(); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (HP <= 0)
            return;

        EnemyManager enemyManager = collision.gameObject.GetComponent<EnemyManager>();

        if (enemyManager != null && !enemyManager.IsZombieDeath())
        {
            DecrementHP(enemyManager.Damage);
        }
    }

    public void DecrementHP(int damage)
    {
        if (Object.HasInputAuthority)
        {
            HP -= damage;
            RPC_SyncHP(HP);
            UpdateHPUI();
        }
    }

    public void IncrementHP(int plus)
    {
        if (Object.HasInputAuthority)
        {
            HP += plus;
            RPC_SyncHP(HP);
            UpdateHPUI();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SyncHP(int newHP)
    {
        HP = newHP;
    }

    private void UpdateHPUI()
    {
        if (Object.HasInputAuthority && _hpText != null)  
        {
            _hpText.text = $"HP: {HP} / {_hpMax}";
        }
    }

    private void FixedUpdate()
    {
        if (HP <= 0 && !_isDied)
        {
            PlayerDied();
        }
    }

    public void PlayerDied()
    {
        _isDied = true;
        _animator.SetBool(DiedAnimationHash, _isDied);

        Player player = GetComponent<Player>();
        if (player != null)
        {
            player.SwitchCameras(false); 
        }
    }

    public bool IsPlayerDied()
    {
        return _isDied;
    }
}
