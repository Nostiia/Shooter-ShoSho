using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private Sprite[] _weaponSprites;
    [SerializeField] private SpriteRenderer _weaponRenderer;

    [Networked] private int _assignedWeaponIndex { get; set; }
    [Networked] private int _hostWeaponIndex { get; set; }

    private static List<int> _assignedWeapons = new List<int>();

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            _assignedWeaponIndex = AssignWeapon();
            _hostWeaponIndex = _assignedWeaponIndex; 
            _assignedWeapons.Add(_assignedWeaponIndex);

            RPC_SyncWeaponIndex(_hostWeaponIndex);
        }
        else
        {
            RPC_RequestWeaponIndex();
        }
        UpdateWeapon(_assignedWeaponIndex);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestWeaponIndex()
    {
        RPC_SyncWeaponIndex(_hostWeaponIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SyncWeaponIndex(int hostWeaponIndex)
    {
        _hostWeaponIndex = hostWeaponIndex;

        if (!Object.HasStateAuthority)
        {
            do
            {
                _assignedWeaponIndex = GetUniqueRandomWeaponIndex();
            }
            while (_assignedWeaponIndex == _hostWeaponIndex); 
        }
    }

    public int GetWeaponIndex()
    {
        return _assignedWeaponIndex; 
    }

    public int AssignWeapon()
    {
        int newWeaponIndex = GetUniqueRandomWeaponIndex();
        return newWeaponIndex;
    }

    public int GetUniqueRandomWeaponIndex()
    {
        if (_assignedWeapons.Count >= _weaponSprites.Length)
        {
            _assignedWeapons.Clear();
        }

        int chosenIndex;
        do
        {
            chosenIndex = Random.Range(0, _weaponSprites.Length);
        }
        while (_assignedWeapons.Contains(chosenIndex));

        _assignedWeapons.Add(chosenIndex); 
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
