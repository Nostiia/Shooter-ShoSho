using System.Collections;
using UnityEngine;
using Fusion;
using TMPro;

public class TimerManager : NetworkBehaviour
{
    [Networked] private float CountdownTimer { get; set; } = 61f; 
    private TMP_Text _timerText;
    private ZombieAppearenceManager _zombieManager;
    private bool isTimerRunning = false;

    public override void Spawned()
    {
        _timerText = GameObject.Find("Timer")?.GetComponent<TMP_Text>();
        _zombieManager = FindObjectOfType<ZombieAppearenceManager>();

        if (Object.HasStateAuthority)
        {
            isTimerRunning = false;
        }
    }

    public void StartTimer()
    {
        if (!isTimerRunning && Object.HasStateAuthority)
        {
            isTimerRunning = true;
            StartCoroutine(TimerRoutine());
        }
    }

    private IEnumerator TimerRoutine()
    {
        while (CountdownTimer > 0)
        {
            CountdownTimer -= 1f;
            RPC_UpdateTimer(CountdownTimer); // Send the updated time to clients

            if (CountdownTimer % 10 == 0)
            {
                _zombieManager?.ZombieSpawned();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateTimer(float time)
    {
        CountdownTimer = time;
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (_timerText)
        {
            int minutes = (int)CountdownTimer / 60;
            int seconds = (int)CountdownTimer % 60;
            _timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
        }
    }
}
