using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShapePickup : MonoBehaviour
{
    [SerializeField] ShapeType shape = ShapeType.Square;   // which power this gives
    [SerializeField] float respawnDelay = 3f;              // seconds before it comes back

    Collider col;
    Renderer[] renderers;

    void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;   // the player passes through it
        renderers = GetComponentsInChildren<Renderer>();
    }

    // Player touched it: give the power and start the respawn
    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerPowers powers)) return;
        powers.Collect(shape);
        StartCoroutine(Respawn());
    }

    // Shape Respawn
    IEnumerator Respawn()
    {
        SetVisible(false);
        yield return new WaitForSeconds(respawnDelay);
        SetVisible(true);
    }

    void SetVisible(bool on)
    {
        col.enabled = on;
        foreach (var r in renderers) r.enabled = on;
    }
}