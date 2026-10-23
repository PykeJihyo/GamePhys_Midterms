using UnityEngine;

public class PickupSpin : MonoBehaviour
{
    [SerializeField] Vector3 spinAxis = new Vector3(0f, 0f, 1f);  // spin axis
    [SerializeField] float spinSpeed = 60f;     // degrees per second
    [SerializeField] float bobHeight = 0.25f;   // how far it floats up and down (m)
    [SerializeField] float bobSpeed = 2f;       // how fast it floats

    Vector3 startLocal;

    void Awake() { startLocal = transform.localPosition; }

    void Update()
    {
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);
        transform.localPosition = startLocal + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);   // sine wave = smooth up/down
    }
}