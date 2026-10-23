using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtons : MonoBehaviour
{
    [SerializeField] string firstLevelScene = "Level1";
    [SerializeField] string mainMenuScene = "MainMenu";

    // Start Game / Retry / Play Again
    public void PlayFromStart()
    {
        Time.timeScale = 1f;
        GameLives.Refill();
        SceneManager.LoadScene(firstLevelScene);
    }

    // Reload the current level (lives are kept)
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    // Close the game (stops Play mode in the editor)
    public void Quit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}