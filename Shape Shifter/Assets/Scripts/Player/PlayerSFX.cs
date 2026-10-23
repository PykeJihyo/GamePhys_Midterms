using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(PlayerMovement))]
public class PlayerSFX : MonoBehaviour
{
    [Header("Walking")]
    [SerializeField] AudioClip walkLoop;                    // walking sound, loops while running
    [SerializeField, Range(0f, 1f)] float walkVolume = 0.6f;
    [SerializeField] float stopGrace = 0.1f;                // wait before stopping, so turning around doesn't cut it (s)
    [SerializeField] bool silentAsShape = true;             // no walking sound as a shape

    [Header("Death")]
    [SerializeField] AudioClip deathClip;
    [SerializeField, Range(0f, 1f)] float deathVolume = 1f;

    AudioSource source;
    PlayerMovement move;
    PlayerRespawn respawn;
    PlayerPowers powers;
    float notWalkingTime;
    bool walkStarted;   // true after the sound has played once, so it resumes instead of restarting
    Vector3 lastPos;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;    // 2D sound
        source.clip = walkLoop;
        source.loop = true;
        source.volume = walkVolume;
        move = GetComponent<PlayerMovement>();
        respawn = GetComponent<PlayerRespawn>();
        powers = GetComponent<PlayerPowers>();
        lastPos = transform.position;
    }

    void OnEnable() { if (respawn != null) respawn.Died += OnDied; }
    void OnDisable() { if (respawn != null) respawn.Died -= OnDied; }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f)   // game paused: pause the sound too
        {
            if (source.isPlaying) source.Pause();
            return;
        }

        // Real speed from how far we moved
        float speed = Mathf.Abs(transform.position.x - lastPos.x) / dt;
        lastPos = transform.position;

        bool inShape = powers != null && (powers.HasJumpPad || powers.HasWallPad);
        bool walking = move.enabled && move.IsGrounded && speed > 0.5f && !(silentAsShape && inShape);

        if (walking)
        {
            notWalkingTime = 0f;
            if (!source.isPlaying)
            {
                if (walkStarted) source.UnPause();   // continue where it stopped
                else { source.Play(); walkStarted = true; }
            }
        }
        else
        {
            notWalkingTime += dt;
            if (notWalkingTime >= stopGrace && source.isPlaying) source.Pause();
        }
    }

    // Stop walking sound and play the death sound
    void OnDied()
    {
        source.Stop();
        walkStarted = false;
        if (deathClip != null) source.PlayOneShot(deathClip, deathVolume);
    }
}