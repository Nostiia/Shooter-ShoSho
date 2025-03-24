using Fusion;
using TMPro;
using UnityEngine;

public class TimerManager : NetworkBehaviour
{
    [Networked] private TickTimer CurrentWaveTimer { get; set; }
    [Networked] private TickTimer RelaxTimer { get; set; }

    [SerializeField] private TMP_Text _timerText;

    [SerializeField] private EnemyAppearenceManager[] _enemyManager;

    [SerializeField] private BoxesSpawnerManager _medKitManager;
    [SerializeField] private BoxesSpawnerManager _ammoPlusManager;
    [SerializeField] private BoxesSpawnerManager _bombManager;

    private bool _isRelaxTime = false;
    private int _lastSpawnCheck = -1; 

    private int _currentWave = 0;
    private int _currentRelaxTime = 0;
    private readonly int[] _waveDurations = { 35, 60, 90 };
    private readonly int[] _relaxDurations = { 10, 20, 30 };

    [SerializeField] private GameObject _winScreen;
    [SerializeField] private GameObject _loseScreen;
    [SerializeField] private GameObject _gameCanvas;

    public void StartGame()
    {
        if (!Object.HasStateAuthority) return;

        _currentWave = 0;
        _currentRelaxTime = 0;
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (_currentWave >= _waveDurations.Length)
        {
            GameOver(true);
            return;
        }
        CurrentWaveTimer = TickTimer.CreateFromSeconds(Runner, _waveDurations[_currentWave]);
        _isRelaxTime = false;
        _lastSpawnCheck = -1;
    }

    private void StartRelaxTimer()
    {
        if (!Object.HasStateAuthority) return;
        RelaxTimer = TickTimer.CreateFromSeconds(Runner, _relaxDurations[_currentWave]);
        _isRelaxTime = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (!_isRelaxTime)
        {
            HandleWaveTimer();
        }
        else
        {
            HandleRelaxTimer();
        }
    }

    private void HandleWaveTimer()
    {
        if (CurrentWaveTimer.IsRunning && !AreBothPlayersDead())
        {
            if (CurrentWaveTimer.Expired(Runner))
            {
                StartRelaxTimer();
            }
            else
            {
                int remainingTime = Mathf.CeilToInt(CurrentWaveTimer.RemainingTime(Runner) ?? 0);
                HandleSpawning(remainingTime);
                RPC_UpdateTimer(remainingTime);
            }
        }
        else
        {
            Debug.LogWarning("Wave timer not running properly.");
        }

        if (AreBothPlayersDead())
        {
            GameOver(false);
        }
    }

    private void HandleSpawning(int remainingTime)
    {
        if (remainingTime > 0 && remainingTime % 10 == 0 && remainingTime != _lastSpawnCheck)
        {
            _ammoPlusManager?.BoxSpawned();
            SpawnEnemiesForWave(_currentWave);
            _lastSpawnCheck = remainingTime;
        }
    }

    private void SpawnEnemiesForWave(int wave)
    {
        switch (wave)
        {
            case 0:
                _enemyManager[0].ZombieSpawned();
                break;
            case 1:
                _medKitManager?.BoxSpawned();
                _enemyManager[0].ZombieSpawned();
                _enemyManager[1].ZombieSpawned();
                break;
            case 2:
                _medKitManager?.BoxSpawned();
                _enemyManager[0].ZombieSpawned();
                _bombManager.BoxSpawned();
                _enemyManager[1].ZombieSpawned();
                _enemyManager[2].ZombieSpawned();
                break;
        }
    }

    private void HandleRelaxTimer()
    {
        if (RelaxTimer.IsRunning)
        {
            if (RelaxTimer.Expired(Runner))
            {
                _currentWave++;
                _currentRelaxTime++;
                StartNextWave();
            }
            else
            {
                int remainingTime = Mathf.CeilToInt(RelaxTimer.RemainingTime(Runner) ?? 0);
                RPC_UpdateTimer(remainingTime);
            }
        }
    }

    private bool AreBothPlayersDead()
    {
        PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();
        bool allPlayersDead = true;

        foreach (PlayerHealth player in players)
        {
            if (!player.IsDead())
            {
                allPlayersDead = false;
                break;
            }
        }

        return allPlayersDead;
    }

    public void GameOver(bool isPlayersAlive)
    {
        if (isPlayersAlive)
        {
            Player[] players = FindObjectsOfType<Player>();
            foreach (Player player in players)
            {
                player.GetComponent<PlayerMovement>().DisableMovement();
            }
            _gameCanvas.SetActive(false);
            _winScreen.SetActive(true);
            ResultManager _winResult = _winScreen.transform.GetComponent<ResultManager>();
            _winResult.ShowResult();
            RPC_WinScreen();
        }
        else
        {
            _gameCanvas.SetActive(false);
            _loseScreen.SetActive(true);
            ResultManager _loseResult = _loseScreen.transform.GetComponent<ResultManager>();
            _loseResult.ShowResult();
            RPC_LoseScreen();
        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateTimer(int time)
    {
        if (_timerText)
        {
            int minutes = time / 60;
            int seconds = time % 60;
            _timerText.text = $"{minutes:0}:{seconds:00}";
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_WinScreen()
    {
        _gameCanvas.SetActive(false);
        _winScreen.SetActive(true);
        ResultManager _winResult = _winScreen.transform.GetComponent<ResultManager>();
        _winResult.ShowResult();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_LoseScreen()
    {
        _gameCanvas.SetActive(false);
        _loseScreen.SetActive(true);
        ResultManager _loseResult = _loseScreen.transform.GetComponent<ResultManager>();
        _loseResult.ShowResult();
    }
}
