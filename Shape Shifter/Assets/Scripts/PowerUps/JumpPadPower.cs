using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerPowers))]
public class JumpPadPower : MonoBehaviour
{
    [SerializeField] float padHeight = 7f;            // normal launch height (m)
    [SerializeField] float momentumMultiplier = 1f;   // multiplies sideways speed when carrying momentum

    [Header("High-fall bounce")]
    [SerializeField] float highBounceThreshold = 7f;  // fall at least this far to bounce back up (m)
    [SerializeField] float highBounceKeep = 1f;       // 1 = same height back

    PlayerMovement move;
    PlayerPowers powers;
    float peakUp;     // highest point since leaving the ground
    float lastSign;   // gravity direction last frame

    void Awake()
    {
        move = GetComponent<PlayerMovement>();
        powers = GetComponent<PlayerPowers>();
        lastSign = move.GravitySign;
        peakUp = UpPosition();
    }

    // Height measured away from gravity (flips when gravity flips)
    float UpPosition() => move.GravitySign * transform.position.y;

    void Update()
    {
        float up = UpPosition();

        // Gravity flipped: start measuring again
        if (move.GravitySign != lastSign)
        {
            lastSign = move.GravitySign;
            peakUp = up;
        }

        // In the air: remember the highest point
        if (!move.IsGrounded)
        {
            peakUp = Mathf.Max(peakUp, up);
            return;
        }

        // On the ground with the power: launch with v = √(2gh)
        if (powers.HasJumpPad)
        {
            float fallHeight = peakUp - up;
            float height = fallHeight >= highBounceThreshold ? fallHeight * highBounceKeep : padHeight;
            float v = Mathf.Sqrt(2f * move.Gravity * height);

            bool momentum = move.CarryingMomentum;
            float vx = momentum ? move.Velocity.x * momentumMultiplier : move.Velocity.x;

            move.Launch(new Vector3(vx, move.GravitySign * v, 0f), 0f, momentum);
            powers.UseJumpPad();
        }

        peakUp = up;   // reset on the ground
    }
}