using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to ComputerScience and Musical letter GameObjects.
/// Detects the first grab via Rigidbody.isKinematic rising edge (OVRGrabbable pattern).
/// Chosen letter glows; unchosen letter floats up and fades out.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class NotificationLetter : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private bool isComputerScience = true;

    [Header("On Chosen — Glow")]
    [SerializeField] private Renderer letterRenderer;
    [SerializeField] private Color emissionColor = Color.cyan;

    [Header("On Chosen — Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip chosenSFX;

    [Header("Other Letter (the unchosen one)")]
    [SerializeField] private GameObject otherLetter;

    [Header("Reject Animation")]
    [SerializeField] private float floatUpSpeed = 0.4f;
    [SerializeField] private float fadeDuration  = 3f;

    private Rigidbody _rb;
    private bool _wasKinematic;
    private bool _chosen;

    private void Awake() => _rb = GetComponent<Rigidbody>();
    private void Start() => _wasKinematic = _rb.isKinematic;

    private void Update()
    {
        if (_chosen) return;

        bool kinematic = _rb.isKinematic;
        if (!_wasKinematic && kinematic)    // rising edge = just grabbed by controller
            OnFirstGrab();
        _wasKinematic = kinematic;
    }

    private void OnFirstGrab()
    {
        _chosen = true;

        // Glow emission
        if (letterRenderer != null)
        {
            letterRenderer.material.EnableKeyword("_EMISSION");
            letterRenderer.material.SetColor("_EmissionColor", emissionColor);
        }

        // Selection sound
        if (chosenSFX != null && audioSource != null)
            audioSource.PlayOneShot(chosenSFX);

        // Record choice in GameManager
        GameManager.Instance?.RecordEvent1Choice(isComputerScience);

        // Trigger reject on the other letter
        otherLetter?.GetComponent<NotificationLetter>()?.StartRejectAnimation();

        Debug.Log($"[NotificationLetter] Chosen: {(isComputerScience ? "ComputerScience" : "Music")}");
    }

    public void StartRejectAnimation() => StartCoroutine(RejectCoroutine());

    private IEnumerator RejectCoroutine()
    {
        // Prevent player from grabbing the unchosen letter mid-animation
        var grabbable = GetComponent<OVRGrabbable>();
        if (grabbable != null) grabbable.enabled = false;

        // Stop physics so gravity doesn't fight the float-up
        _rb.isKinematic = true;

        Material mat = letterRenderer != null ? letterRenderer.material : null;
        Color startColor = mat != null ? mat.color : Color.white;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * floatUpSpeed * Time.deltaTime;
            if (mat != null)
                mat.color = Color.Lerp(startColor, Color.black, elapsed / fadeDuration);
            yield return null;
        }

        gameObject.SetActive(false);
        Debug.Log("[NotificationLetter] Unchosen letter faded and hidden.");
    }
}
