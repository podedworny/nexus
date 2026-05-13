using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentLives = 3;
    public int currency = 0;
    
    public float healthMultiplier = 1f;
    public float damageMultiplier = 1f;
    
    private int _savedCurrency = 0;
    private float _savedHealthMultiplier = 1f;
    private float _savedDamageMultiplier = 1f;
    
    public Action OnGameOver;
    public Action OnPlayerDied;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCurrency(int amount)
    {
        currency += amount;
    }

    public bool SpendCurrency(int amount)
    {
        if (currency >= amount)
        {
            currency -= amount;
            return true;
        }
        return false;
    }

    public void SaveCheckpoint()
    {
        _savedCurrency = currency;
        _savedHealthMultiplier = healthMultiplier;
        _savedDamageMultiplier = damageMultiplier;
    }

    public void LoadCheckpoint()
    {
        currency = _savedCurrency;
        healthMultiplier = _savedHealthMultiplier;
        damageMultiplier = _savedDamageMultiplier;
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