using UnityEngine;
using Fusion;
using TMPro;

public class TimerManager : NetworkBehaviour
{
    [Networked] private TickTimer CountdownTimer { get; set; }
    [Networked] private TickTimer RelaxTimer { get; set; }

    private TMP_Text _timerText;
    private ZombieAppearenceManager _zombieManager;
    private bool _isRelaxTime = false;
    private int _lastSpawnCheck = -1; // Track last spawn interval

    public override void Spawned()
    {
        _timerText = GameObject.Find("Timer")?.GetComponent<TMP_Text>();
        _zombieManager = FindObjectOfType<ZombieAppearenceManager>();
    }

    public void StartGameTimer()
    {
        if (!Object.HasStateAuthority) return;

        CountdownTimer = TickTimer.CreateFromSeconds(Runner, 60f);
        _isRelaxTime = false;
        _lastSpawnCheck = -1; // Reset spawn tracker
    }

    private void StartRelaxTimer()
    {
        if (!Object.HasStateAuthority) return;

        RelaxTimer = TickTimer.CreateFromSeconds(Runner, 10f);
        _isRelaxTime = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (!_isRelaxTime)
        {
            if (!CountdownTimer.IsRunning) return; // Ensure timer has started

            if (CountdownTimer.Expired(Runner))
            {
                StartRelaxTimer();
            }
            else
            {
                int remainingTime = Mathf.CeilToInt(CountdownTimer.RemainingTime(Runner) ?? 0);

                // Spawn zombies only if timer is running and at exact 10-second intervals
                if (remainingTime > 0 && remainingTime % 10 == 0 && remainingTime != _lastSpawnCheck)
                {
                    _zombieManager?.ZombieSpawned();
                    _lastSpawnCheck = remainingTime; // Avoid multiple spawns
                }

                RPC_UpdateTimer(remainingTime);
            }
        }
        else
        {
            if (!RelaxTimer.IsRunning) return; // Ensure relax timer has started

            if (RelaxTimer.Expired(Runner))
            {
                StartGameTimer();
            }
            else
            {
                int remainingTime = Mathf.CeilToInt(RelaxTimer.RemainingTime(Runner) ?? 0);
                RPC_UpdateTimer(remainingTime);
            }
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
}
