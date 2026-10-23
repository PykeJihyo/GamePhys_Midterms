using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] GameObject gameOverPanel;

    PlayerRespawn player;

    // Hide the panel and listen for Game Over
    void Start()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        player = FindFirstObjectByType<PlayerRespawn>();
        if (player != null) player.GameOver += Show;
    }

    void OnDestroy()
    {
        if (player != null) player.GameOver -= Show;
    }

    // Show the panel and freeze the game
    void Show()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}