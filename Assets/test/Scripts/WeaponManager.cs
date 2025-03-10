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
        if (Object.HasStateAuthority) // Host picks weapon first
        {
            uint playerId = Object.Id.Raw;
            int newWeaponIndex = AssignWeapon(playerId);
            _assignedWeaponIndex = newWeaponIndex;

            RPC_SetWeapon(newWeaponIndex);
        }
        else
        {
            UpdateWeapon(_assignedWeaponIndex);
        }
    }

    public int GetWeaponIndex()
    {
        return _assignedWeaponIndex;
    }

    public int AssignWeapon(uint playerId)
    {
        int newWeaponIndex = GetUniqueRandomWeaponIndex();
        while (newWeaponIndex == _hostWeaponIndex)
        {
            newWeaponIndex = GetUniqueRandomWeaponIndex();
        }
        _playerWeaponMap[playerId] = newWeaponIndex; // Store assigned weapon
        if (HasStateAuthority)
        {
            _hostWeaponIndex = newWeaponIndex;
        }
        return newWeaponIndex;
    }

    public int GetUniqueRandomWeaponIndex()
    {
        List<int> availableWeapons = new List<int>();

        // Create a list of available weapon indexes
        for (int i = 0; i < _weaponSprites.Length; i++)
        {
            if (!_assignedWeapons.Contains(i)) // Only add unassigned weapons
                availableWeapons.Add(i);
        }

        if (availableWeapons.Count <= 1) // If all weapons are taken, allow duplicates
        {
            _assignedWeapons.Clear(); // Reset tracking if all weapons are assigned
            for (int i = 0; i < _weaponSprites.Length; i++)
                availableWeapons.Add(i);
        }

        int chosenIndex = availableWeapons[Random.Range(0, availableWeapons.Count)];
        _assignedWeapons.Add(chosenIndex); // Mark it as taken

        return chosenIndex;
    }

    private void UpdateWeapon(int weaponIndex)
    {
        _weaponRenderer.sprite = _weaponSprites[weaponIndex];
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetWeapon(int weaponIndex)
    {
        _assignedWeaponIndex = weaponIndex;
        UpdateWeapon(_assignedWeaponIndex);
    }

    public Sprite GetWeaponSprite(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < _weaponSprites.Length)
        {
            return _weaponSprites[weaponIndex];
        }
        return null; 
    }

    public int WeaponSpritesLength()
    {
        return _weaponSprites.Length;
    }
}
