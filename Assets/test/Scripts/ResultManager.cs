using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class ResultManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text[] _player;
    [SerializeField] private TMP_Text[] _resultKills;
    [SerializeField] private TMP_Text[] _damage;
    [SerializeField] private TMP_Text[] _state;
    int index = 0;

    public void ShowResult()
    {
        Player localPlayer = null;

        Player[] players = FindObjectsOfType<Player>();

        foreach (Player p in players)
        {
            if (p.HasInputAuthority) // This checks if the player is local
            {
                localPlayer = p;
                break;
            }
        }


        foreach (Player p in players)
        {
            index = p.GetId() - 1;
            _player[index].text = "Player" + (index + 1).ToString();
            KillsCount kc = p.GetComponent<KillsCount>();
            _resultKills[index].text = kc.GetKillsCount().ToString();
            _damage[index].text = kc.GetDamage().ToString();
            bool died = p.GetComponent<HPCount>().IsPlayerDied();
            _state[index].text = died ? "DEAD" : "SURVIVED";

            if (p == localPlayer)
            {
                _player[index].color = Color.yellow; 
                _resultKills[index].color = Color.yellow;
                _damage[index].color = Color.yellow;
                _state[index].color = Color.yellow; 
            }
            else
            {
                _player[index].color = Color.white;
                _resultKills[index].color = Color.white;
                _damage[index].color = Color.white;
                _state[index].color = Color.white;
            }
        }
    }
}
