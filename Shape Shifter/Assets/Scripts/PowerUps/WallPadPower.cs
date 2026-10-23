using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerPowers))]
public class WallPadPower : MonoBehaviour
{
    [SerializeField] string wallTag = "Wall";     // only walls with this tag work
    [SerializeField] float pushSpeed = 20f;       // sideways launch speed (m/s)
    [SerializeField] float upSpeed = 10f;         // upward launch speed (m/s), height = v²/2g
    [SerializeField] float inputLockTime = 0.4f;  // can't steer back into the wall for this long (s)

    PlayerMovement move;
    PlayerPowers powers;
    Vector3? taggedWallNormal;   // the tagged wall touched this frame

    void Awake()
    {
        move = GetComponent<PlayerMovement>();
        powers = GetComponent<PlayerPowers>();
    }

    void OnEnable() { move.HitWall += OnHitWall; }
    void OnDisable() { move.HitWall -= OnHitWall; }

    // Remember if what we bumped was a tagged wall
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Mathf.Abs(hit.normal.y) < 0.3f && hit.collider.CompareTag(wallTag))
            taggedWallNormal = hit.normal;
    }

    // More momentum if we combo WallPad + JumpPad
    void OnHitWall(Vector3 anyWallNormal)
    {
        Vector3? n = taggedWallNormal;
        taggedWallNormal = null;

        if (!n.HasValue || !powers.HasWallPad) return;
        if (Mathf.Abs(n.Value.x) < 0.5f) return;   // must be a left/right wall

        // The wall's normal points away from the wall, so that's the launch direction
        float awayFromWall = Mathf.Sign(n.Value.x);
        move.Launch(new Vector3(awayFromWall * pushSpeed, move.GravitySign * upSpeed, 0f), inputLockTime, true);
        powers.UseWallPad();
    }
}