using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    private NetworkRunner _runner;
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private GameObject[] _canvases;
    private bool _isHost = false;
    [SerializeField] private TMP_InputField _createRoomNameInput;
    [SerializeField] private TMP_InputField _connectRoomNameInput;

    public async void StartGame(GameMode mode, string roomName)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        Debug.Log("Inside BasicSpawner Start Game");
        _runner = gameObject.AddComponent<NetworkRunner>();
        DontDestroyOnLoad(_runner);

        var runnerSimulatePhysics2D = gameObject.AddComponent<RunnerSimulatePhysics2D>();
        runnerSimulatePhysics2D.ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;

        _runner.ProvideInput = true;
        var scene = SceneRef.FromIndex(1);
        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            PlayerCount = 2,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });       
    }

    public void HostStart()
    {
        string roomName = string.IsNullOrWhiteSpace(_createRoomNameInput.text) ? "DefaultRoom" : _createRoomNameInput.text;
        _isHost = true;
        DeactivateCanvases();
        _loadingPanel.SetActive(true);
        StartGame(GameMode.Host, roomName);
    }
    public void ClientStart()
    {
        string roomName = string.IsNullOrWhiteSpace(_connectRoomNameInput.text) ? "DefaultRoom" : _connectRoomNameInput.text;
        _isHost = false;
        DeactivateCanvases();
        _loadingPanel.SetActive(true);
        StartGame(GameMode.Client, roomName);
    }

    private void DeactivateCanvases()
    {
        foreach (GameObject c in _canvases)
        {
            c.SetActive(false);
        }
    }

    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector2 spawnPosition = new Vector2((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1);

            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
            WeaponManager weaponManager = networkPlayerObject.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                int uniqueWeaponIndex = weaponManager.GetUniqueRandomWeaponIndex();
                weaponManager.RPC_SetWeapon(uniqueWeaponIndex);
            }
            foreach (var _player in FindObjectsOfType<Player>())
            {
                _player.RPC_HostSelectAvatar();
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    private bool _mouseButton1;

    private void Update()
    {
        _mouseButton1 = _mouseButton1 || Input.GetMouseButton(1);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();   
        if (Input.GetKey(KeyCode.W))
            data.direction += new Vector3(0, 1, 0); // Move up in Y-axis

        if (Input.GetKey(KeyCode.S))
            data.direction += new Vector3(0, -1, 0); // Move down in Y-axis

        if (Input.GetKey(KeyCode.A))
            data.direction += new Vector3(-1, 0, 0); // Move left in X-axis

        if (Input.GetKey(KeyCode.D))
            data.direction += new Vector3(1, 0, 0); // Move right in X-axis

        data.buttons.Set(NetworkInputData.MOUSEBUTTON1, _mouseButton1);
        _mouseButton1 = false;

        input.Set(data);
    }

    public int PlayerCount()
    {
        return _runner.SessionInfo.PlayerCount;
    }
    public bool IsPlayerHost()
    {
        return _isHost;
    }
}