using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject deathMenuPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private GameObject normalDeathButtons;
    [SerializeField] private GameObject gameOverButtons;

    public void ShowDeathMenu()
    {
        deathMenuPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        int currentLives = 0;
        if (GameManager.Instance != null)
        {
            currentLives = GameManager.Instance.currentLives;
        }

        int livesLeft = currentLives - 1;

        if (livesLeft > 0)
        {
            titleText.text = "YOU DIED";
            livesText.text = "You have " + livesLeft + " lives remaining...";
            normalDeathButtons.SetActive(true);
            gameOverButtons.SetActive(false);
        }
        else
        {
            titleText.text = "GAME OVER";
            livesText.text = "No lives remaining!";
            normalDeathButtons.SetActive(false);
            gameOverButtons.SetActive(true);
        }
    }

    public void ShowWinMenu()
    {
        deathMenuPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        titleText.text = "YOU WIN";
        livesText.text = "You have survived the nightfall!";
        normalDeathButtons.SetActive(false);
        gameOverButtons.SetActive(true);
    }

    public void Respawn()
    {
        deathMenuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }
    }

    public void SaveAndQuit()
    {
        PlayerPrefs.SetInt("HasSave", 1);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                PlayerPrefs.SetFloat("PlayerX", stats.StartPosition.x);
                PlayerPrefs.SetFloat("PlayerY", stats.StartPosition.y);
                PlayerPrefs.SetFloat("PlayerZ", stats.StartPosition.z);
                PlayerPrefs.SetInt("PlayerCurrency", stats.currency);
                PlayerPrefs.SetFloat("PlayerHealth", stats.maxHealth * (GameManager.Instance != null ? GameManager.Instance.healthMultiplier : 1f));
            }
        }

        if (GameManager.Instance != null)
        {
            PlayerPrefs.SetInt("PlayerLives", GameManager.Instance.currentLives - 1);
        }

        if (WaveManager.Instance != null)
        {
            PlayerPrefs.SetInt("CurrentWave", WaveManager.Instance.currentWave);
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
    }

    public void LeaveToLobby()
    {
        SceneManager.LoadScene(0);
    }
}
