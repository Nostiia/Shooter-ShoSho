using UnityEngine;
using Fusion;
using TMPro;

public class AmmoCount : NetworkBehaviour
{
    [Networked] private int Ammos { get; set; } = 20;

    private TMP_Text _ammoText;
    private int _ammoMax;
    private const string AmmoText = "Ammo";
    private void Start()
    {
        _ammoText = GameObject.Find(AmmoText)?.GetComponent<TMP_Text>();
        _ammoMax = Ammos;
        if (_ammoText == null)
        {
            Debug.LogError("AmmoText UI not found in the scene!");
        }
    }
    public void DecrementAmmo()
    {
        if (Object.HasStateAuthority)
        {
            Ammos--;
            UpdateAmmoText();
            RPC_Ammos();
        }
    }

    public void IncrementAmmo(int amount)
    {
        if (Object.HasStateAuthority)
        {
            Ammos += amount;
            UpdateAmmoText();
            RPC_Ammos();
        }
    }

    private void UpdateAmmoText()
    {
        if (_ammoText != null)
        {
            if (Object.HasInputAuthority)
            {
                int ammoToShow = Ammos;
                _ammoText.text = $"Ammo: {ammoToShow.ToString()} / {_ammoMax}";
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Ammos()
    {
        int ammoToShow = Ammos;
        _ammoText.text = $"Ammo: {ammoToShow.ToString()} / {_ammoMax}";
    }

    private void FixedUpdate()
    {
        if (_ammoText != null)
        {
            UpdateAmmoText();
        }
    }
    public int GetCurrentAmmo()
    {
        return Ammos;
    }
}