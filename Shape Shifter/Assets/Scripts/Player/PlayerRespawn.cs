using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerMovement), typeof(CharacterController))]
public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] float respawnDelay = 1f;            // wait before respawning (s)
    [SerializeField] bool refillLivesAtStart = false;    // only ticked in Level 4
    [SerializeField] string firstLevelScene = "Level1";  // scene loaded after Game Over

    PlayerMovement move;
    CharacterController controller;
    PlayerPowers powers;
    GravitySwitchPower gravitySwitch;
    Vector3 spawnPoint;   // where the player starts the level

    public bool IsDead { get; private set; }

    public event System.Action Died;
    public event System.Action Respawned;
    public event System.Action GameOver;

    void Awake()
    {
        move = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
        powers = GetComponent<PlayerPowers>();
        gravitySwitch = GetComponent<GravitySwitchPower>();
        spawnPoint = transform.position;

        if (refillLivesAtStart) GameLives.Refill();
    }

    // Lose a life and freeze the player
    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        GameLives.Lives--;
        move.enabled = false;
        Died?.Invoke();
        StartCoroutine(AfterDeath());
    }

    // After the delay: respawn, or Game Over if no lives left
    IEnumerator AfterDeath()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (GameLives.Lives > 0)
            Respawn();
        else if (GameOver != null)
            GameOver.Invoke();
        else
            RestartGame();
    }

    // Back to the start with normal gravity and no powers
    public void Respawn()
    {
        if (gravitySwitch != null) gravitySwitch.Restore();
        if (powers != null) { powers.UseJumpPad(); powers.UseWallPad(); }

        controller.enabled = false;   // CharacterController has to be off to teleport
        transform.position = spawnPoint;
        controller.enabled = true;

        move.enabled = true;
        move.Launch(Vector3.zero);    // clear leftover speed
        IsDead = false;
        Respawned?.Invoke();
    }

    public void RestartGame()
    {
        GameLives.Refill();
        SceneManager.LoadScene(firstLevelScene);
    }
}