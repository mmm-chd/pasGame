using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Spawner References")]
    public EnemySpawner enemySpawner;
    public CollectibleSpawner collectibleSpawner;

    [Header("Tilemap Setup")]
    public GameObject gridPrefab;
    public Tile floorTile;
    public Tile wallTile;

    private Tilemap floorTilemap;
    private Tilemap wallTilemap;
    private GameObject currentGridInstance;

    [Header("Generation Settings")]
    [Range(50, 500)]
    public int walkSteps = 200;
    public Vector2Int startPosition = Vector2Int.zero;
    [Range(50, 500)]
    public int minFloorTiles = 100;
    [Range(0, 3)]
    public int stampSize = 1;
    [Range(10, 200)]
    public int maxGenerationAttempts = 100;

    void Start()
    {
        if (!ValidateSetup())
        {
            Debug.LogError("LevelGenerator setup is incomplete!");
            return;
        }

        ApplyDifficultySettings();
        GenerateLevelWithRetries();
    }

    public void RegenerateLevel()
    {
        Debug.Log("🔄 Regenerating level...");

        ClearCurrentLevel();
        ApplyDifficultySettings();
        GenerateLevelWithRetries();

        Debug.Log("✅ Level regeneration complete!");
    }

    private void ClearCurrentLevel()
    {
        Debug.Log("🗑️ Clearing current level...");

        if (currentGridInstance != null)
        {
            Destroy(currentGridInstance);
            currentGridInstance = null;
            floorTilemap = null;
            wallTilemap = null;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
            Destroy(enemy);

        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        foreach (GameObject collectible in collectibles)
            Destroy(collectible);

        Portal[] portals = FindObjectsByType<Portal>(FindObjectsSortMode.None);
        foreach (Portal portal in portals)
            Destroy(portal.gameObject);

        BlackholeSkill[] attacks = FindObjectsByType<BlackholeSkill>(FindObjectsSortMode.None);
        foreach (BlackholeSkill attack in attacks)
            Destroy(attack.gameObject);

        PortalSpawner portalSpawner = FindFirstObjectByType<PortalSpawner>();
        if (portalSpawner != null)
            portalSpawner.ResetPortalState();

        if (UISystem.Instance != null)
            UISystem.Instance.ResetWaveState();

        Debug.Log("✅ Level cleared successfully!");
    }

    private void ApplyDifficultySettings()
    {
        if (DifficultyManager.Instance != null)
        {
            walkSteps = DifficultyManager.Instance.GetWalkSteps();

            int newCollectibleCount = DifficultyManager.Instance.GetCollectibleCount();

            if (collectibleSpawner != null)
            {
                collectibleSpawner.collectibleCount = newCollectibleCount;
                Debug.Log($"💎 Collectible count set to: {newCollectibleCount}");
            }

            if (DifficultyManager.Instance.IsBossWave())
            {
                Debug.Log($"⚔️ GENERATING BOSS ARENA - Wave {DifficultyManager.Instance.currentLevel} ⚔️");
                Debug.Log($"   Walk Steps: {walkSteps}");
                Debug.Log($"   Collectibles: {newCollectibleCount}");
            }
            else
            {
                Debug.Log($"📍 Generating Wave {DifficultyManager.Instance.currentLevel}");
                Debug.Log($"   Walk Steps: {walkSteps}");
                Debug.Log($"   Collectibles: {newCollectibleCount}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ DifficultyManager.Instance is NULL!");
        }
    }

    private bool ValidateSetup()
    {
        bool isValid = true;

        if (gridPrefab == null)
        {
            Debug.LogError("Grid Prefab is not assigned!");
            isValid = false;
        }

        if (floorTile == null)
        {
            Debug.LogError("Floor Tile is not assigned!");
            isValid = false;
        }

        if (wallTile == null)
        {
            Debug.LogError("Wall Tile is not assigned!");
            isValid = false;
        }

        return isValid;
    }

    void GenerateLevelWithRetries()
    {
        int attempts = 0;

        while (attempts < maxGenerationAttempts)
        {
            if (floorTilemap != null)
                Destroy(floorTilemap.transform.parent.gameObject);

            currentGridInstance = Instantiate(gridPrefab, Vector3.zero, Quaternion.identity);
            floorTilemap = currentGridInstance.transform.Find("Floor").GetComponent<Tilemap>();
            wallTilemap = currentGridInstance.transform.Find("Wall").GetComponent<Tilemap>();

            if (floorTilemap == null || wallTilemap == null)
            {
                Debug.LogError("Could not find 'Floor' or 'Wall' Tilemaps!");
                return;
            }

            HashSet<Vector2Int> floorPositions = GenerateFloor();
            GenerateWalls(floorPositions);

            if (floorPositions.Count >= minFloorTiles)
            {
                Debug.Log($"✅ Level generated! Floor tiles: {floorPositions.Count}");

                StartCoroutine(SetupLevelAfterGeneration(floorPositions));

                return;
            }
            else
            {
                Debug.Log($"Level too small ({floorPositions.Count} tiles). Retrying...");
                attempts++;
            }
        }

        Debug.LogError($"Failed to generate valid level after {maxGenerationAttempts} attempts.");
    }

    private IEnumerator SetupLevelAfterGeneration(HashSet<Vector2Int> floorPositions)
    {
        Debug.Log("🎬 Starting level setup sequence...");
        Debug.Log($"   Floor positions: {floorPositions.Count}");

        PortalSpawner portalSpawner = FindFirstObjectByType<PortalSpawner>();
        if (portalSpawner != null)
        {
            portalSpawner.SetFloorPositions(floorPositions, startPosition);
            Debug.Log("   ✓ Portal spawner configured");
        }

        MovePlayerToSpawn();
        // === ACTIVATE SAFE ZONE IMMEDIATELY AFTER PLAYER TELEPORT/RESPAWN ===
        if (SafeZoneManager.Instance != null)
        {
            SafeZoneManager.Instance.ActivateSafeZone();
            Debug.Log("   🛡 SafeZone activated after player spawn");
        }

        yield return new WaitForEndOfFrame();

        if (enemySpawner != null)
        {
            int enemyCount = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetEnemyCount() : 0;
            Debug.Log($"   🧟 Spawning enemies... (enemyCount = {enemyCount})");
            enemySpawner.SpawnEnemies(floorPositions, startPosition, enemyCount);
            yield return new WaitForEndOfFrame();
        }
        else
        {
            Debug.LogError("❌ EnemySpawner reference is NULL!");
        }

        if (collectibleSpawner != null)
        {
            Debug.Log("   💎 Spawning collectibles...");
            collectibleSpawner.SpawnCollectibles(floorPositions, startPosition);
            yield return new WaitForEndOfFrame();
        }

        if (UISystem.Instance != null)
            UISystem.Instance.UpdateWaveDisplay();

        yield return new WaitForSeconds(0.5f);

        if (UISystem.Instance != null)
        {
            Debug.Log("   ⚡ Activating wave...");
            UISystem.Instance.ActivateWave();
        }

        Debug.Log("✅ Level setup complete!");
    }

    private void MovePlayerToSpawn()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 spawnWorldPos = new Vector3(startPosition.x + 0.5f, startPosition.y + 0.5f, 0);
            player.transform.position = spawnWorldPos;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            Debug.Log($"   ✓ Player moved to spawn: {spawnWorldPos}");
        }
    }

    private HashSet<Vector2Int> GenerateFloor()
    {
        Vector2Int currentPos = startPosition;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < walkSteps; i++)
        {
            for (int x = -stampSize; x <= stampSize; x++)
            {
                for (int y = -stampSize; y <= stampSize; y++)
                {
                    Vector2Int stampedPos = currentPos + new Vector2Int(x, y);
                    floorPositions.Add(stampedPos);
                    floorTilemap.SetTile((Vector3Int)stampedPos, floorTile);
                }
            }

            currentPos += GetRandomDirection();
        }

        return floorPositions;
    }

    private void GenerateWalls(HashSet<Vector2Int> floorPositions)
    {
        HashSet<Vector2Int> wallCandidatePositions = new HashSet<Vector2Int>();

        foreach (var position in floorPositions)
        {
            foreach (var direction in GetCardinalAndDiagonalDirections())
            {
                Vector2Int neighborPos = position + direction;

                if (!floorPositions.Contains(neighborPos))
                    wallCandidatePositions.Add(neighborPos);
            }
        }

        foreach (var wallPos in wallCandidatePositions)
            wallTilemap.SetTile((Vector3Int)wallPos, wallTile);
    }

    private Vector2Int GetRandomDirection()
    {
        int choice = Random.Range(0, 4);
        switch (choice)
        {
            case 0: return Vector2Int.up;
            case 1: return Vector2Int.down;
            case 2: return Vector2Int.left;
            case 3: return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }

    private List<Vector2Int> GetCardinalAndDiagonalDirections()
    {
        return new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };
    }
}
