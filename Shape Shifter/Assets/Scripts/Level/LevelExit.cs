using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class LevelExit : MonoBehaviour
{
    [SerializeField] string sceneToLoad = "";     // empty = next scene in the build list
    [SerializeField] float checkDistance = 0.3f;  // how close below the feet counts as standing on it (m)

    Collider platform;
    PlayerMovement player;
    CharacterController controller;
    bool used;

    void Awake()
    {
        platform = GetComponent<Collider>();
        player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) controller = player.GetComponent<CharacterController>();
    }

    // Check if the player is standing on this platform
    void Update()
    {
        if (used || player == null || !player.enabled || !player.IsGrounded) return;

        Vector3 center = player.transform.TransformPoint(controller.center);
        Vector3 down = -player.GravitySign * Vector3.up;
        float r = controller.radius * 0.9f;
        float dist = controller.height * 0.5f - r + controller.skinWidth + checkDistance;

        if (Physics.SphereCast(center, r, down, out RaycastHit hit, dist) && hit.collider == platform)
            LoadNext();
    }

    void LoadNext()
    {
        used = true;

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
            return;
        }

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            Debug.Log("Last level finished. Set 'Scene To Load' to your end screen scene.");
    }
}