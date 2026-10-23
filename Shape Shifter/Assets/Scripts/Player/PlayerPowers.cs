using UnityEngine;

public enum ShapeType { Square, Triangle, Circle }

public class PlayerPowers : MonoBehaviour
{
    [SerializeField] float chargeDuration = 10f;   // how long a power lasts if not used

    float jumpPadTimer, wallPadTimer;

    public bool HasJumpPad => jumpPadTimer > 0f;
    public bool HasWallPad => wallPadTimer > 0f;
    public float JumpPadTimeLeft => Mathf.Max(jumpPadTimer, 0f);   // for the HUD
    public float WallPadTimeLeft => Mathf.Max(wallPadTimer, 0f);
    public float ChargeDuration => chargeDuration;

    public event System.Action CircleCollected;   // the Gravity Switch listens to this

    // Count the timers down
    void Update()
    {
        float dt = Time.deltaTime;
        if (jumpPadTimer > 0f) jumpPadTimer -= dt;
        if (wallPadTimer > 0f) wallPadTimer -= dt;
    }

    // Called by a pickup when touched
    public void Collect(ShapeType shape)
    {
        switch (shape)
        {
            case ShapeType.Square: jumpPadTimer = chargeDuration; break;
            case ShapeType.Triangle: wallPadTimer = chargeDuration; break;
            case ShapeType.Circle: CircleCollected?.Invoke(); break;
        }
    }

    // Called when a power is used
    public void UseJumpPad() => jumpPadTimer = 0f;
    public void UseWallPad() => wallPadTimer = 0f;
}