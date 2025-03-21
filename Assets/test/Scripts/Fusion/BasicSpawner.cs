using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using System;
using System.Collections;
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
        _runner = gameObject.AddComponent<NetworkRunner>();
        DontDestroyOnLoad(_runner);

        var runnerSimulatePhysics2D = gameObject.AddComponent<RunnerSimulatePhysics2D>();
        runnerSimulatePhysics2D.ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;

        _runner.ProvideInput = true;
        var scene = SceneRef.FromIndex(1);
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
    private int _nextPlayerID = 1;
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector2 spawnPosition = new Vector2((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1);

            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            _spawnedCharacters.Add(player, networkPlayerObject);

            Player newPlayer = networkPlayerObject.GetComponent<Player>();
            if (newPlayer != null)
            {
                newPlayer.SetID(_nextPlayerID); 
                _nextPlayerID++; 

                if (player != runner.LocalPlayer)
                {
                    newPlayer.RPC_RequestAvatarSelection();
                }
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

    [SerializeField] private Joystick _moveJoystick;
    [SerializeField] private Joystick _shootJoystick;
    private bool _isJoystickActive = false;

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        StartCoroutine(FindJoysticks());
    }

    private IEnumerator FindJoysticks()
    {
        yield return new WaitForSeconds(0.5f); 

        _moveJoystick = GameObject.Find("MovementJoystick")?.GetComponent<Joystick>();
        _shootJoystick = GameObject.Find("ShootJoystick")?.GetComponent<Joystick>();

        if (_moveJoystick != null && _shootJoystick != null)
            _isJoystickActive = true;
    }


    private bool _mouseButton1;

    private void Update()
    {
        _mouseButton1 = _mouseButton1 || Input.GetMouseButton(1);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        float horizontalInput = 0;
        float verticalInput = 0;

        float shootHorizontal = 0;
        float shootVertical = 0;

        if (_isJoystickActive)
        {
            horizontalInput = _moveJoystick.Horizontal;
            verticalInput = _moveJoystick.Vertical;
            data.direction += new Vector3(horizontalInput, verticalInput, 0);

            shootHorizontal = _shootJoystick.Horizontal;
            shootVertical = _shootJoystick.Vertical;
            data.shootDirection += new Vector3(shootHorizontal, shootVertical, 0);
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
                data.direction += new Vector3(0, 1, 0); // Move up in Y-axis

            if (Input.GetKey(KeyCode.S))
                data.direction += new Vector3(0, -1, 0); // Move down in Y-axis

            if (Input.GetKey(KeyCode.A))
                data.direction += new Vector3(-1, 0, 0); // Move left in X-axis

            if (Input.GetKey(KeyCode.D))
                data.direction += new Vector3(1, 0, 0); // Move right in X-axis
        }

        if (_shootJoystick != null && (shootHorizontal != 0 || shootVertical != 0))
        {
            data.buttons.Set(NetworkInputData.SHOOT, true);
        }

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