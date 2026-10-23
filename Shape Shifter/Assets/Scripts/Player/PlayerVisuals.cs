using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerPowers))]
public class PlayerVisuals : MonoBehaviour
{
    enum Form { Human, Square, Triangle, Poly }

    [Header("Hierarchy")]
    [SerializeField] Transform visualRoot;     // parent of all the models, rotates when gravity flips
    [SerializeField] GameObject humanForm;     // character model
    [SerializeField] GameObject squareForm;    // shown with the Jump Pad
    [SerializeField] GameObject triangleForm;  // shown with the Wall Pad
    [SerializeField] GameObject polyForm;      // shown with both

    [Header("Animation")]
    [SerializeField] Animator animator;          // found automatically if empty
    [SerializeField] float crossFade = 0.1f;     // blend time between animations (s)
    [SerializeField] float modelYawOffset = 0f;  // 180 if the model faces backwards
    [SerializeField] float turnSpeed = 900f;     // turning speed (degrees/s)
    [SerializeField] float flipSpeed = 720f;     // upside-down flip speed (degrees/s)

    [Header("Shape effects")]
    [SerializeField] float stretchAmount = 0.25f;  // stretch when moving fast up/down
    [SerializeField] float stretchAtSpeed = 17f;   // speed for full stretch (m/s)
    [SerializeField] float squashOnLand = 0.3f;    // squash when landing
    [SerializeField] float popOnChange = 0.35f;    // size pop when changing shape
    [SerializeField] float settleSpeed = 10f;      // how fast effects go back to normal

    PlayerMovement move;
    PlayerPowers powers;
    PlayerRespawn respawn;

    Form form = Form.Human;
    string currentState = "";
    float squash, pop;
    bool facingRight = true;
    Vector3 lastPos;
    Vector3 squareBase, triangleBase, polyBase;   // original sizes of the shapes

    bool IsDead => respawn != null && respawn.IsDead;

    void Awake()
    {
        move = GetComponent<PlayerMovement>();
        powers = GetComponent<PlayerPowers>();
        respawn = GetComponent<PlayerRespawn>();
        if (animator == null && humanForm != null) animator = humanForm.GetComponentInChildren<Animator>(true);

        if (squareForm) squareBase = squareForm.transform.localScale;
        if (triangleForm) triangleBase = triangleForm.transform.localScale;
        if (polyForm) polyBase = polyForm.transform.localScale;

        lastPos = transform.position;
        ShowForm();
    }

    void OnEnable()
    {
        move.Landed += OnLanded;
        if (respawn != null) respawn.Respawned += OnRespawned;
    }

    void OnDisable()
    {
        move.Landed -= OnLanded;
        if (respawn != null) respawn.Respawned -= OnRespawned;
    }

    // LateUpdate runs after movement, so the visuals match this frame
    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;   // paused

        // Real speed from how far we moved (0 when pushing against a wall)
        float groundSpeed = Mathf.Abs(transform.position.x - lastPos.x) / dt;
        lastPos = transform.position;

        UpdateForm();
        UpdateFacing(dt);
        UpdateAnimation(groundSpeed);
        UpdateShapeEffects(dt);
    }

    // Pick the shape from the powers held
    void UpdateForm()
    {
        Form target = Form.Human;
        if (!IsDead)
        {
            if (powers.HasJumpPad && powers.HasWallPad) target = Form.Poly;
            else if (powers.HasJumpPad) target = Form.Square;
            else if (powers.HasWallPad) target = Form.Triangle;
        }

        if (target == form) return;
        form = target;
        ShowForm();
        pop = popOnChange;
    }

    // Turn on only the current form
    void ShowForm()
    {
        if (humanForm) humanForm.SetActive(form == Form.Human);
        if (squareForm) squareForm.SetActive(form == Form.Square);
        if (triangleForm) triangleForm.SetActive(form == Form.Triangle);
        if (polyForm) polyForm.SetActive(form == Form.Poly);
        currentState = "";   // replay the animation when the model comes back
    }

    GameObject FormObject(Form f) => f switch
    {
        Form.Square => squareForm,
        Form.Triangle => triangleForm,
        Form.Poly => polyForm,
        _ => humanForm
    };

    Vector3 BaseScale(Form f) => f switch
    {
        Form.Square => squareBase,
        Form.Triangle => triangleBase,
        Form.Poly => polyBase,
        _ => Vector3.one
    };

    // Flip upside down with gravity and face the way we're moving
    void UpdateFacing(float dt)
    {
        bool flipped = move.GravitySign < 0f;
        if (!IsDead && Mathf.Abs(move.Velocity.x) > 0.1f) facingRight = move.Velocity.x > 0f;

        Quaternion rootTarget = Quaternion.Euler(0f, 0f, flipped ? 180f : 0f);
        visualRoot.localRotation = Quaternion.RotateTowards(visualRoot.localRotation, rootTarget, flipSpeed * dt);

        if (humanForm == null) return;
        // Upside down swaps left and right, so swap the facing too
        float yaw = ((facingRight != flipped) ? 90f : -90f) + modelYawOffset;
        Transform h = humanForm.transform;
        h.localRotation = Quaternion.RotateTowards(h.localRotation, Quaternion.Euler(0f, yaw, 0f), turnSpeed * dt);
    }

    // Pick the animation from the movement. States are played by name, no transitions needed
    void UpdateAnimation(float groundSpeed)
    {
        if (animator == null || !animator.isActiveAndEnabled) return;

        string state;
        if (IsDead) state = "Die";
        else if (!move.IsGrounded) state = move.Velocity.y * move.GravitySign > 0.1f ? "Jump" : "Fall";
        else if (groundSpeed < 0.1f) state = "Idle";
        else state = "Walk";

        if (state == currentState) return;
        if (currentState == "") animator.Play(state, 0, 0f);
        else animator.CrossFadeInFixedTime(state, crossFade);
        currentState = state;
    }

    // Squash on landing, stretch when moving fast, pop when changing shape
    void UpdateShapeEffects(float dt)
    {
        float settle = 1f - Mathf.Exp(-settleSpeed * dt);
        squash = Mathf.Lerp(squash, 0f, settle);
        pop = Mathf.Lerp(pop, 0f, settle);

        if (form == Form.Human) return;
        GameObject shape = FormObject(form);
        if (shape == null) return;

        float stretch = move.IsGrounded ? 0f
            : Mathf.Clamp01(Mathf.Abs(move.Velocity.y) / stretchAtSpeed) * stretchAmount;
        float k = stretch - squash;   // positive = taller, negative = flatter
        Vector3 b = BaseScale(form);
        shape.transform.localScale = new Vector3(
            b.x * (1f - 0.5f * k + pop),
            b.y * (1f + k + pop),
            b.z * (1f - 0.5f * k + pop));
    }

    void OnLanded() { squash = squashOnLand; }

    // Reset visuals after respawning
    void OnRespawned()
    {
        lastPos = transform.position;
        visualRoot.localRotation = Quaternion.identity;
        currentState = "";
    }
}