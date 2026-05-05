using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;
    public DayNightController dayNightController;
    public EnemySpawner enemySpawner;
    
    public enum GameState { Day, TransitioningToNight, Night, TransitioningToDay }
    public GameState currentState = GameState.Day;
    
    public int currentZombiesAlive = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartWaveTransition()
    {
        if (currentState == GameState.Day)
        {
            currentState = GameState.TransitioningToNight;
            StartCoroutine(TransitionToNightRoutine());
        }
    }

    private IEnumerator TransitionToNightRoutine()
    {
        yield return StartCoroutine(dayNightController.RotateSun(true));
        currentState = GameState.Night;
        enemySpawner.SpawnZombies();
    }

    public void ZombieDied()
    {
        currentZombiesAlive--;
        if (currentState == GameState.Night && currentZombiesAlive <= 0)
        {
            StartCoroutine(WaveEndedRoutine());
        }
    }

    private IEnumerator WaveEndedRoutine()
    {
        yield return new WaitForSeconds(3f);
        currentState = GameState.TransitioningToDay;
        yield return StartCoroutine(dayNightController.RotateSun(false));
        currentState = GameState.Day;
    }
}