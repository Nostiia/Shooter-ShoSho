using UnityEngine;
using Fusion;
using TMPro;

public class TimerManager : NetworkBehaviour
{
    [Networked] private TickTimer CurrentWaveTimer { get; set; }
    [Networked] private TickTimer RelaxTimer { get; set; }

    private TMP_Text _timerText;
    [SerializeField] private EnemyAppearenceManager _zombieManager;
    [SerializeField] private EnemyAppearenceManager _sceletonManager;
    [SerializeField] private EnemyAppearenceManager _strongZombieManager;
    [SerializeField] private MedKitManager _medKitManager;
    [SerializeField] private AmmoPlusManager _ammoPlusManager;
    private bool _isRelaxTime = false;
    private int _lastSpawnCheck = -1; // Track last spawn interval

    private int _currentWave = 0;
    private int _currentRelaxTime = 0;
    private readonly int[] _waveDurations = { 30, 40, 60 };
    private readonly int[] _relaxDurations = { 10, 20, 30 };

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
            GameOver(false);
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
                        
                        _ammoPlusManager?.AmmoPlusSpawned();
                        switch (_currentWave)
                        {
                            case 0:
                                _zombieManager?.ZombieSpawned();
                                _lastSpawnCheck = remainingTime;
                                break;
                            case 1:
                                _medKitManager?.MedkitSpawned();
                                _zombieManager?.ZombieSpawned();
                                _sceletonManager?.ZombieSpawned();
                                _lastSpawnCheck = remainingTime;
                                break;
                            case 2:
                                _medKitManager?.MedkitSpawned();
                                _zombieManager?.ZombieSpawned();
                                _sceletonManager?.ZombieSpawned();
                                _strongZombieManager?.ZombieSpawned();
                                _lastSpawnCheck = remainingTime;
                                break;
                        }
                    }
                    RPC_UpdateTimer(remainingTime);
                }
            }
            else
            {
                // Stop timer when both players are dead
                Debug.Log("Both players are dead. Stopping the timer.");
                GameOver(true);
            }
        }
        else
        {
            if (RelaxTimer.IsRunning)
            {
                if (RelaxTimer.Expired(Runner))
                {
                    _currentWave++; // Move to the next wave
                    _currentRelaxTime++; // Move to the next relax time
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
        Debug.Log("Game Over.");
        // Show panel with results.
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
}
