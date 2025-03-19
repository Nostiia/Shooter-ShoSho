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
    int index = 0;

    public void ShowResult()
    {
        Player[] players = FindObjectsOfType<Player>();

        foreach (Player p in players)
        {
            index = p.GetId() - 1;
            _player[index].text = "Player" + (index + 1).ToString();
            _resultKills[index].text = "Kills: " + p.GetComponent<KillsCount>().GetKillsCount().ToString();
            bool died = p.GetComponent<HPCount>().IsPlayerDied();
            _state[index].text = died ? "DEAD" : "SURVIVED";
        }
    }
}
