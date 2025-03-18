using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class HPCount : NetworkBehaviour
{
    [SerializeField] private Sprite _dethSprite;
    [SerializeField] private SpriteRenderer _dethRenderer;
    [Networked] private int HP { get; set; } = 20;
    private TMP_Text _hpText;
    private int _hpMax;

    private Rigidbody2D _rb;
    private Collider2D _collider;

    private bool _isDied = false;

    private void Start()
    {
        _hpText = GameObject.Find("HP")?.GetComponent<TMP_Text>();
        _hpMax = HP;
        _dethRenderer = transform.Find("Body").GetComponent<SpriteRenderer>();

        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

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
        Player player = transform.transform.GetComponent<Player>();
        if (player != null)
        {
            player.SwitchCameras(false); // Disable camera when the player dies
        }
        _dethRenderer.sprite = _dethSprite;
        _isDied = true;

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
