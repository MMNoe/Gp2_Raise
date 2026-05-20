using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PotionPour : MonoBehaviour
{
    // OVRGrabbable sets the Rigidbody to isKinematic=true when the object is held.
    // Pouring is detected when held AND tilted upside-down (transform.up.y < tiltThreshold).
    [SerializeField] private float tiltThreshold = -0.5f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugTrigger = true;

    private Rigidbody _rb;

    public bool IsPoured  { get; private set; }
    public bool IsHeld    => _rb != null && _rb.isKinematic;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    private void Update()
    {
        if (enableDebugTrigger && Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[PotionPour] Debug key P pressed — force pour state.");
            IsPoured = true;
        }

        bool pouringNow = IsHeld && transform.up.y < tiltThreshold;

        if (pouringNow && !IsPoured)
        {
            IsPoured = true;
            Debug.Log("[PotionPour] Potion is being poured! (held + tilted)");
        }
        else if (!pouringNow && IsPoured && !Input.GetKey(KeyCode.P))
        {
            // Reset when no longer tilting (ignore while debug key held)
            IsPoured = false;
            Debug.Log("[PotionPour] Potion returned to upright — pour stopped.");
        }
    }
}
