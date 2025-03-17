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
            GameOver();
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
            if (!CurrentWaveTimer.IsRunning) return;

            if (CurrentWaveTimer.Expired(Runner))
            {
                StartRelaxTimer();
            }
            else
            {
                int remainingTime = Mathf.CeilToInt(CurrentWaveTimer.RemainingTime(Runner) ?? 0);
                if (remainingTime > 0 && remainingTime % 10 == 0 && remainingTime != _lastSpawnCheck)
                {
                    switch (_currentWave)
                    {
                        case 0:
                            Debug.Log(_currentWave);
                            _zombieManager?.ZombieSpawned();
                            _lastSpawnCheck = remainingTime;
                            break;
                        case 1:
                            Debug.Log(_currentWave);
                            _zombieManager?.ZombieSpawned();
                            _sceletonManager?.ZombieSpawned();
                            _lastSpawnCheck = remainingTime;
                            break;
                        case 2:
                            Debug.Log(_currentWave);
                            break;
                    }      
                }
                RPC_UpdateTimer(remainingTime);
            }
        }
        else
        {
            if (!RelaxTimer.IsRunning) return;

            if (RelaxTimer.Expired(Runner))
            {
                _currentWave++; // Move to the next wave
                _currentRelaxTime++; //Move to the next relax time
                StartNextWave();
            }
            else
            {
                int remainingTime = Mathf.CeilToInt(RelaxTimer.RemainingTime(Runner) ?? 0);
                RPC_UpdateTimer(remainingTime);
            }
        }
    }

    public void GameOver()
    {
        Debug.Log("All waves complete! Game Over.");
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
