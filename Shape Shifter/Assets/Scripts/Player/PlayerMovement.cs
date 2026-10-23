using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Run (Week 2)")]
    [SerializeField] float maxSpeed = 8f;             // top speed (m/s)
    [SerializeField] float timeToMaxSpeed = 0.133f;   // time to reach top speed (s)
    [SerializeField] float timeToStop = 0.1f;         // time to stop (s)

    [Header("Jump (Week 3)")]
    [SerializeField] float gravity = 20f;             // gravity strength (m/s²)
    [SerializeField] float jumpHeight = 3f;           // full jump height (m)
    [SerializeField] float riseAfterRelease = 1f;     // extra height after letting go of Space (m)
    [SerializeField] float coyoteTime = 0.1f;         // can still jump this long after leaving a ledge (s)
    [SerializeField] float jumpBufferTime = 0.12f;    // jump press is remembered this long (s)

    [Header("Ground probe (Week 3/4)")]
    [SerializeField] float groundProbe = 0.05f;       // how far below the feet still counts as ground (m)

    [Header("Momentum")]
    [SerializeField] float momentumGrace = 0.1f;      // time on the ground before Wall Pad speed is lost (s)

    CharacterController controller;
    Vector3 velocity;
    float coyoteTimer, bufferTimer, inputLockTimer, groundedTime;
    Vector3? pendingWallNormal;   // wall touched this frame
    float lockedZ;                // starting Z, so the player stays on the 2D plane
    bool canCutJump;              // only our own jump can be cut short, not power launches

    public float GravitySign { get; private set; } = 1f;   // σ: +1 normal gravity, -1 flipped
    public bool IsGrounded { get; private set; }
    public bool CarryingMomentum { get; private set; }      // keeping Wall Pad speed in the air
    public float Gravity => gravity;
    public float MaxSpeed => maxSpeed;
    public Vector3 Velocity => velocity;

    public event System.Action Landed;            // tells other scripts we landed
    public event System.Action<Vector3> HitWall;  // tells other scripts we hit a wall

    float Accel => maxSpeed / timeToMaxSpeed;               // a = Δv / t
    float Decel => maxSpeed / timeToStop;                   // a = Δv / t
    float JumpSpeed => Mathf.Sqrt(2f * gravity * jumpHeight);   // v0 = √(2gh)

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        lockedZ = transform.position.z;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Input
        float input = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) bufferTimer = jumpBufferTime;

        // Timers
        bufferTimer -= dt;
        inputLockTimer -= dt;
        coyoteTimer = IsGrounded ? coyoteTime : coyoteTimer - dt;

        // move velocity toward the target speed
        if (inputLockTimer <= 0f)
        {
            float target = input * maxSpeed;

            if (CarryingMomentum)
            {
                // Keep the launch speed; only pressing the other way slows you down
                bool braking = input != 0f && Mathf.Sign(input) != Mathf.Sign(velocity.x);
                if (braking) velocity.x = Mathf.MoveTowards(velocity.x, target, Decel * dt);
                if (Mathf.Abs(velocity.x) <= maxSpeed) CarryingMomentum = false;
            }
            else
            {
                float rate = input != 0f ? Accel : Decel;
                velocity.x = Mathf.MoveTowards(velocity.x, target, rate * dt);
            }
        }

        // Jump (times σ so it works upside down)
        if (bufferTimer > 0f && coyoteTimer > 0f)
        {
            velocity.y = GravitySign * JumpSpeed;
            bufferTimer = 0f;
            coyoteTimer = 0f;
            canCutJump = true;
        }

        // small jump: if Space is released while rising, cut speed to vCut = √(2g·h)
        if (canCutJump && !Input.GetButton("Jump"))
        {
            float vCut = Mathf.Sqrt(2f * gravity * riseAfterRelease);
            if (velocity.y * GravitySign > vCut)
                velocity.y = GravitySign * vCut;
            canCutJump = false;
        }

        // Gravity: v = v − σ·g·Δt
        float vyBefore = velocity.y;
        velocity.y -= GravitySign * gravity * dt;

        // Move. Average of old and new speed (trapezoid) keeps jump height the same at any frame rate
        Vector3 step = new Vector3(
            velocity.x * dt,
            (vyBefore + velocity.y) * 0.5f * dt,
            lockedZ - transform.position.z);

        bool wasGrounded = IsGrounded;
        pendingWallNormal = null;
        CollisionFlags flags = controller.Move(step);

        // Ground check. "Floor" is below normally, above when flipped. Moving away = not grounded
        CollisionFlags floorFlag = GravitySign > 0f ? CollisionFlags.Below : CollisionFlags.Above;
        CollisionFlags ceilingFlag = GravitySign > 0f ? CollisionFlags.Above : CollisionFlags.Below;
        bool notRising = velocity.y * GravitySign <= 0f;
        IsGrounded = notRising && ((flags & floorFlag) != 0 || ProbeFloor());

        if (IsGrounded)
            velocity.y = -GravitySign * 1f;   // small push into the floor to stay grounded
        else if ((flags & ceilingFlag) != 0 && velocity.y * GravitySign > 0f)
            velocity.y = 0f;                  // hit head: stop going up

        // Momentum stops when hitting a wall or standing on the ground too long
        if (CarryingMomentum && (flags & CollisionFlags.Sides) != 0)
        {
            velocity.x = 0f;
            CarryingMomentum = false;
        }
        if (CarryingMomentum && IsGrounded)
        {
            groundedTime += dt;
            if (groundedTime >= momentumGrace) CarryingMomentum = false;
        }
        else groundedTime = 0f;

        // Tell the power scripts what happened
        if (IsGrounded && !wasGrounded) Landed?.Invoke();
        if (pendingWallNormal.HasValue) HitWall?.Invoke(pendingWallNormal.Value);
    }

    // Sphere check toward the feet to find the floor (ignores triggers like pickups)
    bool ProbeFloor()
    {
        Vector3 center = transform.TransformPoint(controller.center);
        float r = controller.radius * 0.95f;
        float halfHeight = Mathf.Max(controller.height * 0.5f, controller.radius);
        float dist = halfHeight - r + controller.skinWidth + groundProbe;
        Vector3 down = -GravitySign * Vector3.up;
        return Physics.SphereCast(center, r, down, out _, dist, ~0, QueryTriggerInteraction.Ignore);
    }

    // Called by Unity when we bump something. Mostly sideways normal = a wall
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Mathf.Abs(hit.normal.y) < 0.3f)
            pendingWallNormal = hit.normal;
    }

    // Used by powers to throw the player
    public void Launch(Vector3 newVelocity, float lockTime = 0f, bool keepMomentum = false)
    {
        velocity = new Vector3(newVelocity.x, newVelocity.y, 0f);
        inputLockTimer = lockTime;
        IsGrounded = false;
        coyoteTimer = 0f;
        canCutJump = false;
        CarryingMomentum = keepMomentum;
        groundedTime = 0f;
    }

    // Used by the Gravity Switch. +1 normal, -1 flipped
    public void SetGravitySign(float sign)
    {
        GravitySign = Mathf.Sign(sign);
        IsGrounded = false;
        canCutJump = false;

        // If already moving tow    ard the new floor, reset speed so it doesn't add up with gravity
        if (velocity.y * GravitySign < 0f)
            velocity.y = 0f;
    }
}