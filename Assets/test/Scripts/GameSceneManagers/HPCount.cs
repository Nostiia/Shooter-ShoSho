using Fusion;
using TMPro;
using UnityEngine;

public class HPCount : NetworkBehaviour
{
    [Networked] private int HP { get; set; } = 20;
    private TMP_Text _hpText;
    private int _hpMax;

    private Rigidbody2D _rb;
    private Collider2D _collider;

    private bool _isDied = false;
    private Animator _animator;

    private void Start()
    {
        _hpText = GameObject.Find("HP")?.GetComponent<TMP_Text>();
        _hpMax = HP;
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        _animator = transform.Find("Body").GetComponent<Animator>();

        if (_hpText == null)
        {
            Debug.LogError("HPText UI not found in the scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (HP <= 0)
            return;
        EnemyManager enemyManager = collision.gameObject.GetComponent<EnemyManager>();

        if (enemyManager != null && !enemyManager.IsZombieDeath())
        {
            DecrementHP(enemyManager.GetDamage());
        }
    }

    public void DecrementHP(int damage)
    {
        if (Object.HasStateAuthority)
        {
            HP -= damage;
            UpdateHPText();
            RPC_HP();
        }
    }

    public void IncrementHP(int plus)
    {
        if (Object.HasStateAuthority)
        {
            HP += plus;
            UpdateHPText();
            RPC_HP();
        }
    }

    private void UpdateHPText()
    {
        if (_hpText != null)
        {
            if (Object.HasInputAuthority)
            {
                int hpToShow = HP;
                _hpText.text = $"HP: {hpToShow.ToString()} / {_hpMax}";
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_HP()
    {
        int hpToShow = HP;
        _hpText.text = $"HP: {hpToShow.ToString()} / {_hpMax}";
    }

    private void FixedUpdate()
    {
        if (_hpText != null)
        {
            UpdateHPText();
        }
        if (HP <= 0)
        {
            PlayerDied();
        }
    }

    public void PlayerDied()
    {
        _isDied = true;
        _animator.SetBool("isDied", _isDied);
        Player player = transform.transform.GetComponent<Player>();
        if (player != null)
        {
            player.SwitchCameras(false); // Disable camera when the player dies
        }

        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;  // Stop any existing movement
        }
    }
    
    public bool IsPlayerDied()
    {
        return _isDied;
    }
}
