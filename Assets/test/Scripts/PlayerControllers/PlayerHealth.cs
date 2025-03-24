using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private HPCount _hpCounter;
    private Player _player;
    public bool _isDead { get; private set; } = false;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _hpCounter = GetComponent<HPCount>();
    }

    public void TakeDamage(int damage)
    {
        _hpCounter.DecrementHP(damage);
    }

    public bool IsDead()
    {
        _isDead = _hpCounter.IsPlayerDied();
        return _isDead;
    }

    public PlayerHealth FindAnotherAlivePlayer()
    {
        foreach (var player in FindObjectsOfType<PlayerHealth>())
        {
            if (player != this && !player._isDead)
            {
                return player;
            }
        }
        return null;
    }
}
