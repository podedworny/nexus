using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private Button loadGameButton;

    private void Start()
    {
        if (loadGameButton != null)
        {
            loadGameButton.interactable = PlayerPrefs.GetInt("HasSave", 0) == 1;
        }
    }

    public void StartNewGame()
    {
        PlayerPrefs.SetInt("LoadSave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    public void LoadGame()
    {
        PlayerPrefs.SetInt("LoadSave", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
