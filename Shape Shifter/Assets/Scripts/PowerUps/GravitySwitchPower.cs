using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerPowers))]
public class GravitySwitchPower : MonoBehaviour
{
    [SerializeField] float duration = 10f;          // how long gravity stays flipped (s)
    [SerializeField] KeyCode cancelKey = KeyCode.G; // press to flip back early

    PlayerMovement move;
    PlayerPowers powers;
    float timer;

    public bool IsFlipped => move.GravitySign < 0f;
    public float TimeLeft => IsFlipped ? Mathf.Max(timer, 0f) : 0f;   // for the HUD

    void Awake()
    {
        move = GetComponent<PlayerMovement>();
        powers = GetComponent<PlayerPowers>();
    }

    void OnEnable() { powers.CircleCollected += Flip; }
    void OnDisable() { powers.CircleCollected -= Flip; }

    // Touched a Circle: flip gravity and start the timer
    void Flip()
    {
        move.SetGravitySign(-1f);
        timer = duration;
    }

    // Count down, flip back when time runs out or G is pressed
    void Update()
    {
        if (!IsFlipped) return;

        timer -= Time.deltaTime;
        if (timer <= 0f || Input.GetKeyDown(cancelKey))
            Restore();
    }

    // Back to normal gravity (also used when respawning)
    public void Restore()
    {
        move.SetGravitySign(1f);
    }
}