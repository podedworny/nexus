using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentLives = 3;

    public float healthMultiplier = 1f;
    public float damageMultiplier = 1f;

    private float _savedHealthMultiplier = 1f;
    private float _savedDamageMultiplier = 1f;

    public Action OnGameOver;
    public Action OnPlayerDied;
    public Action OnSaveCheckpoint;
    public Action OnLoadCheckpoint;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SaveCheckpoint()
    {
        _savedHealthMultiplier = healthMultiplier;
        _savedDamageMultiplier = damageMultiplier;
        OnSaveCheckpoint?.Invoke();
    }

    public void LoadCheckpoint()
    {
        healthMultiplier = _savedHealthMultiplier;
        damageMultiplier = _savedDamageMultiplier;
        OnLoadCheckpoint?.Invoke();
    }

    public void LoseLife()
    {
        currentLives--;
        if (currentLives <= 0)
        {
            OnGameOver?.Invoke();
        }
        else
        {
            LoadCheckpoint();
            OnPlayerDied?.Invoke();
        }
    }
}
