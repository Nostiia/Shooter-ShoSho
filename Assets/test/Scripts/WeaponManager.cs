using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private Sprite[] _weaponSprites;
    private int _hostWeaponIndex = 0;
    private SpriteRenderer _weaponRenderer;

    private void Awake()
    {
        _weaponRenderer = transform.Find("Weapon").GetComponent<SpriteRenderer>();
    }

    public override void Spawned()
    {
        _hostWeaponIndex = Random.Range(0, _weaponSprites.Length);
        _weaponRenderer.sprite = _weaponSprites[_hostWeaponIndex];
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_WeaponSpawner()
    {
        RPC_GenerateWeapon(_hostWeaponIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_GenerateWeapon(int weaponNotIncludedIndex)
    {
        int _weaponIndex = Random.Range(0, _weaponSprites.Length);
        while (_weaponIndex == _hostWeaponIndex)
        {
            _weaponIndex = Random.Range(0, _weaponSprites.Length);
        }
        _weaponRenderer.sprite = _weaponSprites[_weaponIndex];

    }
}
