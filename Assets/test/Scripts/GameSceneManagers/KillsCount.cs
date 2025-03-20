using UnityEngine;
using Fusion;
using TMPro;

public class KillsCount : NetworkBehaviour
{
    [Networked] private int HostKills { get; set; } = 0;

    [Networked] private int Damage { get; set; } = 0;

    private TMP_Text _killsText;

    private void Start()
    {
        // Find the text inside the scene dynamically
        _killsText = GameObject.Find("Kills")?.GetComponent<TMP_Text>();

        if (_killsText == null)
        {
            Debug.LogError("KillsText UI not found in the scene!");
        }
    }

    public void AddDamage(int damage)
    {
        Damage += damage;
    }

    public int GetDamage()
    {
        return Damage;
    }

    public void IncrementKills()
    {
        if (Object.HasStateAuthority)
        {
            HostKills++;
            UpdateKillText();
            RPC_Kills();
        }
    }

    private void UpdateKillText()
    {
        if (_killsText != null)
        {
            if (Object.HasInputAuthority)
            {
                int killsToShow = HostKills;
                _killsText.text = "Kills: " + killsToShow.ToString();
            }
        }
    }

    public int GetKillsCount()
    {
        return HostKills;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Kills()
    {
        int killsToShow = HostKills;
        _killsText.text = "Kills: " + killsToShow.ToString();
    }

    private void FixedUpdate()
    {
        if (_killsText != null)
        {
            UpdateKillText();
        }
    }
}
