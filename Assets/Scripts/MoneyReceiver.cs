using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to VAULT02 and PiggyBank GameObjects.
/// Requires a Trigger Collider on the same GameObject sized to cover the deposit opening.
/// Detects CoinToken objects entering the trigger and notifies GameManager.
/// Uses instance ID deduplication so each coin is counted only once per container.
/// </summary>
public class MoneyReceiver : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private bool isVault = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip depositClip;

    private readonly HashSet<int> _depositedIDs = new HashSet<int>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<CoinToken>(out _)) return;

        int id = other.gameObject.GetInstanceID();
        if (!_depositedIDs.Add(id)) return;     // already counted this coin

        if (depositClip != null && audioSource != null)
            audioSource.PlayOneShot(depositClip);

        Debug.Log($"[MoneyReceiver] Coin deposited into {(isVault ? "VAULT" : "PIGGY")}.");
        GameManager.Instance?.RecordCoinDeposit(isVault);
    }
}
