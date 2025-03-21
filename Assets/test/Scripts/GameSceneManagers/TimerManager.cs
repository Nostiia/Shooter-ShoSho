using Fusion;
using TMPro;
using UnityEngine;

public class TimerManager : NetworkBehaviour
{
    [Networked] private TickTimer CurrentWaveTimer { get; set; }
    [Networked] private TickTimer RelaxTimer { get; set; }

    private TMP_Text _timerText;
    [SerializeField] private EnemyAppearenceManager _zombieManager;
    [SerializeField] private EnemyAppearenceManager _sceletonManager;
    [SerializeField] private EnemyAppearenceManager _strongZombieManager;
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

    public override void Spawned()
    {
        _timerText = GameObject.Find("Timer")?.GetComponent<TMP_Text>();
    }

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
            if (CurrentWaveTimer.IsRunning && !AreBothPlayersDead())
            {
                if (CurrentWaveTimer.Expired(Runner))
                {
                    StartRelaxTimer();
                }
                else
                {
                    int remainingTime = Mathf.CeilToInt(CurrentWaveTimer.RemainingTime(Runner) ?? 0);
                    if (remainingTime > 0 && remainingTime % 10 == 0 && remainingTime != _lastSpawnCheck)
                    {
                        
                        _ammoPlusManager?.BoxSpawned();
                        switch (_currentWave)
                        {
                            case 0:
                                _zombieManager?.ZombieSpawned();
                                _lastSpawnCheck = remainingTime;
                                break;
                            case 1:
                                _medKitManager?.BoxSpawned();
                                _zombieManager?.ZombieSpawned();
                                _sceletonManager?.ZombieSpawned();
                                _lastSpawnCheck = remainingTime;
                                break;
                            case 2:
                                _medKitManager?.BoxSpawned();
                                _zombieManager?.ZombieSpawned();
                                _bombManager?.BoxSpawned();
                                _sceletonManager?.ZombieSpawned();
                                _strongZombieManager?.ZombieSpawned();
                                _lastSpawnCheck = remainingTime;
                                break;
                        }
                    }
                    RPC_UpdateTimer(remainingTime);
                }
            }
            if (AreBothPlayersDead())
            {
                GameOver(false);
            }
        }
        else
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
    }

    private bool AreBothPlayersDead()
    {
        Player[] players = FindObjectsOfType<Player>();
        bool allPlayersDead = true;

        foreach (Player player in players)
        {
            if (player.IsPlayerAlive())
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
                player.DisableMovement();
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
