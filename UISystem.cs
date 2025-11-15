using System.Collections;
using TMPro;
using UnityEngine;

public class UISystem : MonoBehaviour
{
    public static UISystem Instance;

    [Header("UI Text References")]
    [SerializeField] public TMP_Text enemiesQuantityText;
    public TMP_Text attackCooldownText;
    public TMP_Text waveNumberText;

    [Header("Portal Spawning")]
    public PortalSpawner portalSpawner;

    [Header("Settings")]
    [Tooltip("Delay before showing 'Search for portal' message")]
    public float messageDelay = 2f;

    [Tooltip("How often to update enemy count (in seconds)")]
    public float updateInterval = 0.3f;

    public int enemiesQuantity;
    private int previousEnemyCount = -1;
    private bool hasShownClearedMessage = false;
    private float updateTimer = 0f;

    // ✅ TAMBAHKAN FLAG UNTUK WAVE READY
    private bool waveIsActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(InitialEnemyCountUpdate());
        UpdateWaveDisplay();
    }

    void Update()
    {
        // ✅ HANYA UPDATE JIKA WAVE SUDAH AKTIF
        if (!waveIsActive) return;

        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateEnemyCount();
        }
    }

    private IEnumerator InitialEnemyCountUpdate()
    {
        // ✅ TUNGGU LEBIH LAMA UNTUK MEMASTIKAN SEMUA MUSUH SUDAH SPAWN
        yield return new WaitForSeconds(0.5f);

        hasShownClearedMessage = false;
        waveIsActive = true; // ✅ AKTIFKAN WAVE

        UpdateEnemyCount();

        Debug.Log($"✅ Wave started! Initial enemy count: {enemiesQuantity}");
    }

    public void UpdateEnemyCount()
    {
        if (!waveIsActive) return; // ✅ SKIP JIKA WAVE BELUM AKTIF

        int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (currentEnemyCount != previousEnemyCount)
        {
            previousEnemyCount = currentEnemyCount;
            enemiesQuantity = currentEnemyCount;

            Debug.Log($"🔍 Enemy count: {enemiesQuantity}");

            if (enemiesQuantity == 0 && !hasShownClearedMessage)
            {
                hasShownClearedMessage = true;
                waveIsActive = false; // ✅ NONAKTIFKAN WAVE
                StartCoroutine(ShowClearedMessage());
            }
            else if (enemiesQuantity > 0)
            {
                enemiesQuantityText.text = "Enemies: " + enemiesQuantity.ToString();
            }
        }
    }

    public void OnEnemyDied()
    {
        UpdateEnemyCount();
    }

    IEnumerator ShowClearedMessage()
    {
        Debug.Log("✅ All enemies cleared! Starting portal spawn...");

        if (DifficultyManager.Instance != null && DifficultyManager.Instance.IsBossWave())
        {
            enemiesQuantityText.text = "🏆 BOSS DEFEATED! 🏆";
        }
        else
        {
            enemiesQuantityText.text = "All enemies cleared!!";
        }

        yield return new WaitForSeconds(messageDelay);

        enemiesQuantityText.text = "Search for the portal";

        if (portalSpawner != null)
        {
            Debug.Log("🌀 Spawning portal...");
            portalSpawner.SpawnPortal();
        }
        else
        {
            Debug.LogError("❌ PortalSpawner reference is missing!");
        }
    }

    public void UpdateCooldownDisplay(float remainingCooldown)
    {
        if (remainingCooldown <= 0f)
        {
            attackCooldownText.text = "Attack: READY";
            attackCooldownText.color = Color.green;
        }
        else
        {
            attackCooldownText.text = $"Attack: {remainingCooldown:F1}s";
            attackCooldownText.color = Color.yellow;
        }
    }

    public void UpdateWaveDisplay()
    {
        if (waveNumberText != null && DifficultyManager.Instance != null)
        {
            if (DifficultyManager.Instance.IsBossWave())
            {
                waveNumberText.text = $"⚔️ BOSS WAVE {DifficultyManager.Instance.currentLevel} ⚔️";
                waveNumberText.color = Color.red;
            }
            else
            {
                waveNumberText.text = $"Wave {DifficultyManager.Instance.currentLevel}";
                waveNumberText.color = Color.white;
            }
        }
    }

    /// <summary>
    /// ✅ RESET UI STATE SAAT WAVE BARU
    /// </summary>
    public void ResetWaveState()
    {
        hasShownClearedMessage = false;
        previousEnemyCount = -1;
        waveIsActive = false; // ✅ NONAKTIFKAN DULU
        Debug.Log("✅ UI wave state reset");
    }

    /// <summary>
    /// ✅ AKTIFKAN WAVE SETELAH ENEMIES SPAWN
    /// Dipanggil dari LevelGenerator setelah semua spawn selesai
    /// </summary>
    public void ActivateWave()
    {
        waveIsActive = true;
        UpdateEnemyCount();
        Debug.Log("✅ Wave activated!");
    }
}