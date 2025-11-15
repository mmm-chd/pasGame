using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PortalSpawner : MonoBehaviour
{
    [Header("Portal Setup")]
    [Tooltip("The portal prefab to spawn")]
    public GameObject portalPrefab;

    [Tooltip("Spawn portal at this position (optional)")]
    public Transform spawnPoint;

    [Tooltip("If true, spawns at random floor position")]
    public bool spawnAtRandomPosition = true;

    [Tooltip("Offset from ground (Z-axis for 2D games)")]
    public float spawnHeightOffset = 0f;

    [Header("Safe Zone Settings")]
    [Tooltip("Minimum distance from player spawn")]
    public float minDistanceFromPlayer = 5f;

    private bool portalSpawned = false;
    private HashSet<Vector2Int> floorPositions;
    private Vector2Int playerStartPosition;

    // Store floor positions from LevelGenerator
    public void SetFloorPositions(HashSet<Vector2Int> positions, Vector2Int playerSpawn = default)
    {
        floorPositions = positions;
        playerStartPosition = playerSpawn;

        Debug.Log($"PortalSpawner received {positions?.Count ?? 0} floor positions. Player spawn: {playerSpawn}");
    }

    // Call this when all enemies are cleared
    public void SpawnPortal()
    {
        if (portalSpawned)
        {
            Debug.LogWarning("Portal already spawned!");
            return;
        }

        if (portalPrefab == null)
        {
            Debug.LogError("Portal prefab is missing!");
            return;
        }

        Vector3 spawnPosition;

        if (spawnAtRandomPosition)
        {
            // Use stored floor positions (best method)
            if (floorPositions != null && floorPositions.Count > 0)
            {
                spawnPosition = GetRandomFloorPositionAwayFromPlayer();
                Debug.Log($"✅ Portal spawning at valid floor position: {spawnPosition}");
            }
            // Fallback: Try to find floor tilemap
            else if (TryGetFloorFromTilemap(out Vector3 tilemapPosition))
            {
                spawnPosition = tilemapPosition;
                Debug.Log($"⚠️ Portal spawning from tilemap fallback: {spawnPosition}");
            }
            // Last resort: spawn at world center
            else
            {
                Debug.LogWarning("No floor positions available! Spawning portal at (0,0,0)");
                spawnPosition = new Vector3(0, 0, spawnHeightOffset);
            }
        }
        else if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
        }
        else
        {
            spawnPosition = new Vector3(0, 0, spawnHeightOffset);
        }

        GameObject portal = Instantiate(portalPrefab, spawnPosition, Quaternion.identity);
        portalSpawned = true;

        Debug.Log($"✅ Portal spawned at {spawnPosition}! Enter to proceed to next wave.");
    }

    /// <summary>
    /// Get random position from stored floor positions, away from player
    /// FIXED: Proper 2D coordinate conversion
    /// </summary>
    private Vector3 GetRandomFloorPositionAwayFromPlayer()
    {
        if (floorPositions == null || floorPositions.Count == 0)
        {
            Debug.LogError("No floor positions available!");
            return Vector3.zero;
        }

        // Filter positions that are far enough from player
        List<Vector2Int> validPositions = floorPositions
            .Where(pos => Vector2Int.Distance(pos, playerStartPosition) >= minDistanceFromPlayer)
            .ToList();

        // If no valid positions, use all positions
        if (validPositions.Count == 0)
        {
            Debug.LogWarning($"No positions far from player (min distance: {minDistanceFromPlayer})! Using any floor position.");
            validPositions = floorPositions.ToList();
        }

        // Pick random position
        int randomIndex = Random.Range(0, validPositions.Count);
        Vector2Int randomPos = validPositions[randomIndex];

        // FIXED: Convert tile position to world position correctly for 2D
        // In 2D top-down games: X = horizontal, Y = vertical, Z = depth/layer
        Vector3 worldPosition = new Vector3(
            randomPos.x + 0.5f,  // Center of tile (X axis)
            randomPos.y + 0.5f,  // Center of tile (Y axis)
            spawnHeightOffset    // Z-axis offset (usually 0 for 2D)
        );

        // Verify position is still in floor set (safety check)
        if (!floorPositions.Contains(randomPos))
        {
            Debug.LogError($"Selected position {randomPos} is not in floor set!");
        }

        return worldPosition;
    }

    /// <summary>
    /// Try to get random floor position from tilemap directly
    /// FIXED: Better tilemap detection and 2D positioning
    /// </summary>
    private bool TryGetFloorFromTilemap(out Vector3 position)
    {
        position = Vector3.zero;

        // Find the floor tilemap
        UnityEngine.Tilemaps.Tilemap floorTilemap = FindFloorTilemap();
        if (floorTilemap == null)
        {
            Debug.LogWarning("Could not find floor tilemap!");
            return false;
        }

        // Get all tiles in the tilemap
        List<Vector3Int> floorTilePositions = new List<Vector3Int>();
        BoundsInt bounds = floorTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (floorTilemap.HasTile(cellPosition))
                {
                    floorTilePositions.Add(cellPosition);
                }
            }
        }

        if (floorTilePositions.Count > 0)
        {
            // Filter positions away from player
            List<Vector3Int> validPositions = floorTilePositions
                .Where(pos => Vector2.Distance(new Vector2(pos.x, pos.y), playerStartPosition) >= minDistanceFromPlayer)
                .ToList();

            if (validPositions.Count == 0)
            {
                validPositions = floorTilePositions; // Use any position if none are far enough
            }

            Vector3Int randomCell = validPositions[Random.Range(0, validPositions.Count)];
            position = floorTilemap.GetCellCenterWorld(randomCell);
            position.z = spawnHeightOffset; // Set proper Z-axis

            Debug.Log($"Found {floorTilePositions.Count} floor tiles in tilemap. Selected: {randomCell}");
            return true;
        }

        Debug.LogWarning("No floor tiles found in tilemap!");
        return false;
    }

    /// <summary>
    /// Find the floor tilemap in the scene
    /// FIXED: Better error handling and search methods
    /// </summary>
    private UnityEngine.Tilemaps.Tilemap FindFloorTilemap()
    {
        // Method 1: Find by name "Floor"
        GameObject floorObject = GameObject.Find("Floor");
        if (floorObject != null)
        {
            var tilemap = floorObject.GetComponent<UnityEngine.Tilemaps.Tilemap>();
            if (tilemap != null)
            {
                Debug.Log("Found floor tilemap by name 'Floor'");
                return tilemap;
            }
        }

        // Method 2: Find all Tilemaps and search for floor
        UnityEngine.Tilemaps.Tilemap[] allTilemaps = FindObjectsByType<UnityEngine.Tilemaps.Tilemap>(FindObjectsSortMode.None);
        foreach (var tilemap in allTilemaps)
        {
            if (tilemap.gameObject.name.ToLower().Contains("floor"))
            {
                Debug.Log($"Found floor tilemap: {tilemap.gameObject.name}");
                return tilemap;
            }
        }

        // Method 3: Find Grid and search its children
        UnityEngine.Tilemaps.GridInformation grid = FindFirstObjectByType<UnityEngine.Tilemaps.GridInformation>();
        if (grid != null)
        {
            foreach (Transform child in grid.transform)
            {
                if (child.name.ToLower().Contains("floor"))
                {
                    var tilemap = child.GetComponent<UnityEngine.Tilemaps.Tilemap>();
                    if (tilemap != null)
                    {
                        Debug.Log($"Found floor tilemap in Grid children: {child.name}");
                        return tilemap;
                    }
                }
            }
        }

        Debug.LogError("Could not find floor tilemap by any method!");
        return null;
    }

    // Public method to reset portal state
    public void ResetPortalState()
    {
        portalSpawned = false;
        Debug.Log("Portal state reset");
    }

    // Debug method to visualize spawn area
    private void OnDrawGizmos()
    {
        if (floorPositions != null && floorPositions.Count > 0)
        {
            // Draw floor positions
            Gizmos.color = Color.green;
            foreach (var pos in floorPositions)
            {
                Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
                Gizmos.DrawWireCube(worldPos, Vector3.one * 0.9f);
            }

            // Draw player spawn area
            Gizmos.color = Color.red;
            Vector3 playerWorldPos = new Vector3(playerStartPosition.x + 0.5f, playerStartPosition.y + 0.5f, 0);
            Gizmos.DrawWireSphere(playerWorldPos, minDistanceFromPlayer);
        }
    }
}