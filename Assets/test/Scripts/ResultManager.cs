using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class ResultManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text[] _player;
    [SerializeField] private TMP_Text[] _resultKills;
    [SerializeField] private TMP_Text[] _state;

    private int count = 0;
    public void ShowResult()
    {
        Player[] players = FindObjectsOfType<Player>();

        foreach (Player p in players)
        {
            _player[count].text = $"Player{count+1}";
            _resultKills[count].text = "Kills: " + p.GetComponent<KillsCount>().GetKillsCount().ToString();
            bool died = p.GetComponent<HPCount>().IsPlayerDied();
            _state[count].text = died ? "DEAD" : "SURVIVED";
            count++;
        }
    }
}
