using Fusion;
using TMPro;
using UnityEngine;

public class ResultManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text[] _player;
    [SerializeField] private TMP_Text[] _resultKills;
    [SerializeField] private TMP_Text[] _damage;
    [SerializeField] private TMP_Text[] _state;
    private int index = 0;

    public void ShowResult()
    {
        Player localPlayer = null;

        Player[] players = FindObjectsOfType<Player>();

        foreach (Player p in players)
        {
            if (p.HasInputAuthority)
            {
                localPlayer = p;
                break;
            }
        }


        foreach (Player p in players)
        {
            index = p.Id - 1;
            _player[index].text = "Player" + (index + 1).ToString();
            KillsCount kc = p.GetComponent<KillsCount>();
            _resultKills[index].text = kc.GetKillsCount().ToString();
            _damage[index].text = kc.GetDamage().ToString();
            bool died = p.GetComponent<HPCount>().IsPlayerDied();
            _state[index].text = died ? "DEAD" : "SURVIVED";

            if (p == localPlayer)
            {
                SetColoring(index, Color.yellow);
            }
            else
            {
                SetColoring(index, Color.white);
            }
        }
    }

    private void SetColoring (int index, Color color)
    {
        _player[index].color = color;
        _resultKills[index].color = color;
        _damage[index].color = color;
        _state[index].color = color;
    }
}
