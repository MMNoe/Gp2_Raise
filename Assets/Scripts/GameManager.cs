using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ─── Enums ─────────────────────────────────────────────────────
    public enum Event1Result { None, ComputerScience, Music }
    public enum Event2Result { None, Saved, Ignored }
    public enum Event3Result { None, Vault, Piggy }

    public Event1Result Event1 { get; private set; }
    public Event2Result Event2 { get; private set; }
    public Event3Result Event3 { get; private set; }

    // ─── Inspector ─────────────────────────────────────────────────
    [Header("Timer")]
    [SerializeField] private float totalTime = 180f;
    [SerializeField] private float warningTime = 30f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip warningVoiceClip;
    [SerializeField] private AudioClip fragmentRevealClip;

    [Header("Portfolio Fragments (assign 3, start inactive in scene)")]
    [SerializeField] private GameObject fragment1;
    [SerializeField] private GameObject fragment2;
    [SerializeField] private GameObject fragment3;

    [Header("Debug")]
    [SerializeField] private bool enableDebugTrigger = true;

    // ─── Internal ──────────────────────────────────────────────────
    private float _timeRemaining;
    private bool _timerRunning;
    private bool _warningPlayed;
    private int _fragmentsRevealed;
    private int _vaultCoins;
    private int _piggyCoins;
    private int _totalCoinsDeposited;
    private const int TotalCoins = 3;

    // ─── Lifecycle ─────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()  => CatManager.OnCatRescued += HandleCatRescued;
    private void OnDisable() => CatManager.OnCatRescued -= HandleCatRescued;

    private void Start()
    {
        _timeRemaining = totalTime;
        _timerRunning = true;
        fragment1?.SetActive(false);
        fragment2?.SetActive(false);
        fragment3?.SetActive(false);
        Debug.Log("[GameManager] 180s timer started.");
    }

    private void Update()
    {
        if (enableDebugTrigger) HandleDebugInput();
        if (!_timerRunning) return;

        _timeRemaining -= Time.deltaTime;

        if (!_warningPlayed && _timeRemaining <= warningTime)
        {
            _warningPlayed = true;
            if (warningVoiceClip != null && audioSource != null)
                audioSource.PlayOneShot(warningVoiceClip);
            Debug.Log("[GameManager] 30s warning played.");
        }

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _timerRunning = false;
            OnTimerExpired();
        }
    }

    // ─── Timer End ─────────────────────────────────────────────────
    private void OnTimerExpired()
    {
        Debug.Log("[GameManager] Timer expired.");

        if (Event2 == Event2Result.None)
        {
            Event2 = Event2Result.Ignored;
            Debug.Log("[GameManager] Event 2: Cat IGNORED (no fragment reveal).");
        }

        if (Event3 == Event3Result.None && _totalCoinsDeposited > 0)
            FinalizeEvent3();
    }

    // ─── Event 1: Letter ───────────────────────────────────────────
    public void RecordEvent1Choice(bool isComputerScience)
    {
        if (Event1 != Event1Result.None) return;
        Event1 = isComputerScience ? Event1Result.ComputerScience : Event1Result.Music;
        Debug.Log($"[GameManager] Event 1: {Event1}");
        RevealNextFragment();
    }

    // ─── Event 2: Cat ──────────────────────────────────────────────
    private void HandleCatRescued()
    {
        if (Event2 != Event2Result.None) return;
        Event2 = Event2Result.Saved;
        Debug.Log("[GameManager] Event 2: Cat SAVED.");
        RevealNextFragment();
    }

    // ─── Event 3: Coins ────────────────────────────────────────────
    public void RecordCoinDeposit(bool isVault)
    {
        if (Event3 != Event3Result.None) return;
        if (isVault) _vaultCoins++; else _piggyCoins++;
        _totalCoinsDeposited++;
        Debug.Log($"[GameManager] Coin deposited. Vault={_vaultCoins} Piggy={_piggyCoins}");
        if (_totalCoinsDeposited >= TotalCoins) FinalizeEvent3();
    }

    private void FinalizeEvent3()
    {
        if (Event3 != Event3Result.None) return;
        Event3 = (_vaultCoins >= _piggyCoins) ? Event3Result.Vault : Event3Result.Piggy;
        Debug.Log($"[GameManager] Event 3: {Event3} (Vault={_vaultCoins}, Piggy={_piggyCoins})");
        RevealNextFragment();
    }

    // ─── Fragments ─────────────────────────────────────────────────
    private void RevealNextFragment()
    {
        _fragmentsRevealed++;
        GameObject frag = _fragmentsRevealed switch
        {
            1 => fragment1,
            2 => fragment2,
            3 => fragment3,
            _ => null
        };

        if (frag == null)
        {
            Debug.LogWarning($"[GameManager] Fragment {_fragmentsRevealed} not assigned in Inspector.");
            return;
        }

        frag.SetActive(true);
        if (fragmentRevealClip != null && audioSource != null)
            audioSource.PlayOneShot(fragmentRevealClip);
        Debug.Log($"[GameManager] Fragment {_fragmentsRevealed} revealed.");
    }

    // ─── Scene 3 Properties (read by Scene 3 scripts) ─────────────
    public bool Scene3ShowCS    => Event1 == Event1Result.ComputerScience;
    public bool Scene3ShowVault => Event3 == Event3Result.Vault;
    public float TimeRemaining  => _timeRemaining;

    // ─── Debug (Editor only) ───────────────────────────────────────
    private void HandleDebugInput()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1)) { Debug.Log("[GameManager][Debug] Force Event1=CS"); RecordEvent1Choice(true); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { Debug.Log("[GameManager][Debug] Force Event2=Saved"); HandleCatRescued(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { Debug.Log("[GameManager][Debug] Force coin→Vault"); RecordCoinDeposit(true); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { Debug.Log("[GameManager][Debug] Force coin→Piggy"); RecordCoinDeposit(false); }
        if (Input.GetKeyDown(KeyCode.T))      { Debug.Log("[GameManager][Debug] Skip to 31s"); _timeRemaining = 31f; }
#endif
    }
}
