using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject[] gameUiElements;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject deathMenuPanel;

    private bool isPaused = false;

    private void Update()
    {
        if (deathMenuPanel != null && deathMenuPanel.activeSelf)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (inventoryPanel != null && inventoryPanel.activeSelf)
            {
                inventoryPanel.SetActive(false);
                return;
            }

            if (shopPanel != null && shopPanel.activeSelf)
            {
                shopPanel.SetActive(false);
                return;
            }

            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);

        foreach (GameObject element in gameUiElements)
        {
            if (element != null) element.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);

        foreach (GameObject element in gameUiElements)
        {
            if (element != null) element.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt("HasSave", 1);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", player.transform.position.z);

            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                PlayerPrefs.SetInt("PlayerCurrency", stats.currency);
                PlayerPrefs.SetFloat("PlayerHealth", stats.currentHealth);
            }
        }

        if (GameManager.Instance != null)
        {
            PlayerPrefs.SetInt("PlayerLives", GameManager.Instance.currentLives);
        }

        if (WaveManager.Instance != null)
        {
            PlayerPrefs.SetInt("CurrentWave", WaveManager.Instance.currentWave);
        }

        PlayerPrefs.Save();
    }

    public void SaveAndQuit()
    {
        SaveGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
