using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }
    
    [Header("Level Progression")]
    public int currentLevel = 1;
    
    [Header("Enemy Scaling Settings")]
    [Tooltip("Base enemy count at level 1")]
    public int baseEnemyCount = 5;
    
    [Tooltip("How many enemies increase per level")]
    public int enemyIncreasePerLevel = 2;
    
    [Tooltip("Enemy multiplier for boss waves")]
    public float bossWaveEnemyMultiplier = 1.5f;
    
    [Header("Collectible Scaling Settings")]
    [Tooltip("Base collectible count at level 1")]
    public int baseCollectibleCount = 3;
    
    [Tooltip("How many collectibles increase per level")]
    public int collectibleIncreasePerLevel = 1;
    
    [Header("Map Size Scaling Settings")]
    [Tooltip("Base walk steps at level 1")]
    public int baseWalkSteps = 150;
    
    [Tooltip("How many walk steps increase per level")]
    public int walkStepsIncreasePerLevel = 20;
    
    [Tooltip("Fixed walk steps for boss arenas (medium size)")]
    public int bossArenaWalkSteps = 250;
    
    [Header("Boss Wave Settings")]
    [Tooltip("Levels where boss waves occur")]
    public int[] bossWaveLevels = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
    
    [Header("Optional")]
    [Tooltip("Boss enemy prefab (optional)")]
    public GameObject bossEnemyPrefab;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ DifficultyManager initialized");
            LogCurrentStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // WAVE TYPE DETECTION
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    
    public bool IsBossWave()
    {
        foreach (int bossLevel in bossWaveLevels)
        {
            if (currentLevel == bossLevel)
                return true;
        }
        return false;
    }
    
    public bool IsBossWaveEveryX(int interval = 5)
    {
        return currentLevel % interval == 0;
    }
    
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // SCALING CALCULATIONS
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    
    /// <summary>
    /// Calculate map size (walk steps)
    /// </summary>
    public int GetWalkSteps()
    {
        if (IsBossWave())
        {
            return bossArenaWalkSteps;
        }
        
        return baseWalkSteps + (walkStepsIncreasePerLevel * (currentLevel - 1));
    }
    
    /// <summary>
    /// Calculate enemy count based on current level
    /// FORMULA: baseEnemyCount + (enemyIncreasePerLevel × (level - 1))
    /// </summary>
    public int GetEnemyCount()
    {
        int count = baseEnemyCount + (enemyIncreasePerLevel * (currentLevel - 1));
        
        if (IsBossWave())
        {
            count = Mathf.RoundToInt(count * bossWaveEnemyMultiplier);
        }
        
        return count;
    }
    
    /// <summary>
    /// Calculate collectible count
    /// </summary>
    public int GetCollectibleCount()
    {
        int count = baseCollectibleCount + (collectibleIncreasePerLevel * (currentLevel - 1));
        
        if (IsBossWave())
        {
            count = Mathf.RoundToInt(count * 1.5f);
        }
        
        return count;
    }
    
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // LEVEL PROGRESSION
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    
    /// <summary>
    /// Advance to next level
    /// </summary>
    public void NextLevel()
    {
        currentLevel++;
        
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        if (IsBossWave())
        {
            Debug.Log($"⚔️ ADVANCED TO BOSS WAVE {currentLevel}! ⚔️");
        }
        else
        {
            Debug.Log($"📈 Advanced to Wave {currentLevel}");
        }
        
        LogCurrentStats();
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }
    
    /// <summary>
    /// Reset progress back to level 1
    /// </summary>
    public void ResetProgress()
    {
        currentLevel = 1;
        Debug.Log("🔄 Progress reset to Level 1");
        LogCurrentStats();
    }

    /// <summary>
    /// Log current difficulty stats (for debugging)
    /// </summary>
    public void LogCurrentStats()
    {
        Debug.Log($"━━━ Wave {currentLevel} Stats ━━━");
        Debug.Log($"  Boss Wave: {IsBossWave()}");
        Debug.Log($"  Enemies: {GetEnemyCount()}");
        Debug.Log($"  Collectibles: {GetCollectibleCount()}");
        Debug.Log($"  Map Size: {GetWalkSteps()} steps");
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }
    
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // PREVIEW CALCULATIONS (untuk UI/debugging)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    
    /// <summary>
    /// Preview stats for a specific level
    /// </summary>
    public void PreviewLevel(int level)
    {
        int oldLevel = currentLevel;
        currentLevel = level;
        
        Debug.Log($"━━━ Preview Wave {level} ━━━");
        Debug.Log($"  Boss Wave: {IsBossWave()}");
        Debug.Log($"  Enemies: {GetEnemyCount()}");
        Debug.Log($"  Collectibles: {GetCollectibleCount()}");
        Debug.Log($"  Map Size: {GetWalkSteps()} steps");
        
        currentLevel = oldLevel;
    }
}