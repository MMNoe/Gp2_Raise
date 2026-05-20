using UnityEngine;

public class InjuredCat : MonoBehaviour
{
    [SerializeField] private Color injuredColor = new Color(0.4f, 0.3f, 0.3f);
    [SerializeField] private Color healedColor  = new Color(0.8f, 0.5f, 0.2f);

    [Header("Debug")]
    [SerializeField] private bool enableDebugTrigger = true;

    private Renderer _renderer;
    private bool _healed;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
            _renderer.material.color = injuredColor;
    }

    private void Update()
    {
        if (enableDebugTrigger && Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("[InjuredCat] Debug key H pressed — force heal.");
            Heal();
        }
    }

    // Cat has a Sphere Collider (Is Trigger=true). Potion has a Rigidbody.
    private void OnTriggerStay(Collider other)
    {
        if (_healed) return;
        if (!other.TryGetComponent<PotionPour>(out var potion)) return;
        if (!potion.IsPoured) return;

        Debug.Log("[InjuredCat] Potion detected while pouring — healing cat!");
        Heal();
    }

    public void Heal()
    {
        if (_healed) return;
        _healed = true;

        if (_renderer != null)
            _renderer.material.color = healedColor;

        // Stand up: reset rotation for placeholder capsule lying on its side
        transform.rotation = Quaternion.identity;

        Debug.Log("[InjuredCat] Cat healed — notifying CatManager.");
        CatManager.Instance?.OnCatSaved();
    }
}
