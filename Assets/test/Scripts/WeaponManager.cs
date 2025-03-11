using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private Sprite[] _weaponSprites;
    private SpriteRenderer _weaponRenderer;

    [Networked] private int _assignedWeaponIndex { get; set; }
    private static List<int> _assignedWeapons = new List<int>();
    private Dictionary<uint, int> _playerWeaponMap = new Dictionary<uint, int>();

    private int _hostWeaponIndex = -1;

    private void Awake()
    {
        _weaponRenderer = transform.Find("Weapon").GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _assignedWeapons.Clear();
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            _assignedWeaponIndex = AssignWeapon();
        }
        else
        {
            while (_assignedWeaponIndex == _hostWeaponIndex)
            {
                _assignedWeaponIndex = GetUniqueRandomWeaponIndex();
            }
            RPC_RequestWeaponIndex();
        }
        UpdateWeapon(_assignedWeaponIndex);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestWeaponIndex()
    {
        RPC_SyncWeaponIndex(_assignedWeaponIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SyncWeaponIndex(int weaponIndex)
    {
        _assignedWeaponIndex = weaponIndex;
        UpdateWeapon(_assignedWeaponIndex);
    }

    public int GetWeaponIndex()
    {
        return _assignedWeaponIndex; 
    }

    public int AssignWeapon()
    {
        int newWeaponIndex = GetUniqueRandomWeaponIndex();
        if (HasStateAuthority)
        {
            _hostWeaponIndex = newWeaponIndex;
        }
        return newWeaponIndex;
    }

    public int GetUniqueRandomWeaponIndex()
    {
        int chosenIndex = Random.Range(0, _weaponSprites.Length);

        return chosenIndex;
    }

    private void UpdateWeapon(int weaponIndex)
    {
        _weaponRenderer.sprite = _weaponSprites[weaponIndex];
    }

    public Sprite GetWeaponSprite(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < _weaponSprites.Length)
        {
            return _weaponSprites[weaponIndex];
        }
        return null; 
    }
}
