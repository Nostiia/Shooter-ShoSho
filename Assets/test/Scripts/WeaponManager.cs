using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private Sprite[] _weaponSprites;
    private int _hostWeaponIndex = 0;
    private SpriteRenderer _weaponRenderer;

    [SerializeField] private Sprite[] _shotsSprites;
    [SerializeField] private PhysxBall _prefabPhysxBall;
    private SpriteRenderer _shotRenderer;

    private void Awake()
    {
        _weaponRenderer = transform.Find("Weapon").GetComponent<SpriteRenderer>();
        _shotRenderer = _prefabPhysxBall.transform.Find("Circle").GetComponent <SpriteRenderer>();
    }

    public override void Spawned()
    {
        _hostWeaponIndex = Random.Range(0, _weaponSprites.Length);
        _weaponRenderer.sprite = _weaponSprites[_hostWeaponIndex];
        _shotRenderer.sprite = _shotsSprites[_hostWeaponIndex];
    }

    public int WeaponIndex()
    {
        return _hostWeaponIndex;
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
        _shotRenderer.sprite = _shotsSprites [_weaponIndex];

    }
}
