using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;

public class StartManager : MonoBehaviour
{
    [SerializeField] private BasicSpawner _spawner;
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _startButton;
    [SerializeField] private GameObject _startCanvas;
    private void Start()
    {
        _spawner = FindObjectOfType<BasicSpawner>();
        _startButton.SetActive(false);
    }

    private void Update()
    {
        if ( _spawner != null  && _spawner.PlayerCount() == 1 && _spawner.IsPlayerHost())
        {
            _startButton.SetActive(true);
        }
    }

    public void StartGame()
    {
        _startCanvas.SetActive(false);
        foreach (var player in FindObjectsOfType<Player>())
        {
            player.RPC_CanMove(true);
        }

    }
}
