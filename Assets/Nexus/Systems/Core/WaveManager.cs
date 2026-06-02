using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;
    public DayNightController dayNightController;
    public EnemySpawner enemySpawner;

    public enum GameState { Day, TransitioningToNight, Night, TransitioningToDay }
    public GameState currentState = GameState.Day;

    public int currentZombiesAlive = 0;
    public int currentWave = 1;
    public int maxWaves = 30;

    public bool showDebugUI = true;
    public InputAction toggleDebugUIAction;
    public InputAction killAllZombiesAction;
    private bool isDebugMenuOpen = false;
    private string debugWaveString = "1";

    private PlayerStats _playerStats;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerStats = player.GetComponent<PlayerStats>();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDied += ResetWave;
            GameManager.Instance.SaveCheckpoint();
        }
    }

    private void OnEnable()
    {
        toggleDebugUIAction.Enable();
        killAllZombiesAction.Enable();
    }

    private void OnDisable()
    {
        toggleDebugUIAction.Disable();
        killAllZombiesAction.Disable();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDied -= ResetWave;
        }
    }

    private void Update()
    {
        if (toggleDebugUIAction.WasPressedThisFrame() && showDebugUI && currentState == GameState.Day)
        {
            isDebugMenuOpen = !isDebugMenuOpen;
            if (isDebugMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        if (killAllZombiesAction.WasPressedThisFrame() && showDebugUI && currentState == GameState.Night)
        {
            ZombieHealth[] allZombies = FindObjectsByType<ZombieHealth>(FindObjectsSortMode.None);
            foreach (ZombieHealth zombie in allZombies)
            {
                zombie.TakeDamage(999999f);
            }
        }
    }

    public void StartWaveTransition()
    {
        if (currentState == GameState.Day && currentWave <= maxWaves)
        {
            isDebugMenuOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            currentState = GameState.TransitioningToNight;
            StartCoroutine(TransitionToNightRoutine());
        }
    }

    private IEnumerator TransitionToNightRoutine()
    {
        bool isBloodMoon = currentWave % 10 == 0;
        yield return StartCoroutine(dayNightController.RotateSun(true, isBloodMoon));
        currentState = GameState.Night;
        enemySpawner.SpawnZombies(currentWave);
    }

    public void ZombieDied()
    {
        currentZombiesAlive--;
        if (_playerStats != null)
        {
            _playerStats.AddCurrency(10);
        }

        if (currentState == GameState.Night && currentZombiesAlive <= 0)
        {
            StartCoroutine(WaveEndedRoutine());
        }
    }

    private IEnumerator WaveEndedRoutine()
    {
        yield return new WaitForSeconds(3f);
        currentState = GameState.TransitioningToDay;
        yield return StartCoroutine(dayNightController.RotateSun(false, false));
        currentWave++;
        currentState = GameState.Day;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveCheckpoint();
        }
    }

    private void ResetWave()
    {
        StopAllCoroutines();

        ZombieHealth[] allZombies = FindObjectsByType<ZombieHealth>(FindObjectsSortMode.None);
        foreach (ZombieHealth zombie in allZombies)
        {
            Destroy(zombie.gameObject);
        }

        currentZombiesAlive = 0;
        currentState = GameState.TransitioningToDay;
        StartCoroutine(dayNightController.RotateSun(false, false));
        currentState = GameState.Day;
    }

    private void OnGUI()
    {
        if (!showDebugUI || !isDebugMenuOpen || currentState != GameState.Day) return;

        GUIStyle debugBoxStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 14
        };

        GUILayout.BeginArea(new Rect(10, 10, 150, 80), debugBoxStyle);
        GUILayout.Label("Debug Wave:");
        debugWaveString = GUILayout.TextField(debugWaveString);

        if (GUILayout.Button("Set Wave"))
        {
            if (int.TryParse(debugWaveString, out int parsedWave))
            {
                currentWave = Mathf.Clamp(parsedWave, 1, maxWaves);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveCheckpoint();
                }
            }
        }
        GUILayout.EndArea();
    }
}
