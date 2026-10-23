using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] KeyCode altPauseKey = KeyCode.P;   // backup, browsers can take over Esc

    public static bool IsPaused { get; private set; }

    PlayerRespawn player;
    PlayerMovement move;
    GravitySwitchPower gravity;

    void Start()
    {
        player = FindFirstObjectByType<PlayerRespawn>();
        if (player != null)
        {
            move = player.GetComponent<PlayerMovement>();
            gravity = player.GetComponent<GravitySwitchPower>();
        }
        SetPaused(false);
    }

    void Update()
    {
        if (!Input.GetKeyDown(pauseKey) && !Input.GetKeyDown(altPauseKey)) return;
        if (player != null && player.IsDead) return;   // can't pause while dead
        SetPaused(!IsPaused);
    }

    public void Resume() => SetPaused(false);   // Resume button

    // timeScale 0 freezes the game. Player scripts are turned off so key presses don't count
    void SetPaused(bool paused)
    {
        IsPaused = paused;
        if (pausePanel) pausePanel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;

        if (move) move.enabled = !paused;
        if (gravity) gravity.enabled = !paused;
    }

    void OnDestroy()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }
}