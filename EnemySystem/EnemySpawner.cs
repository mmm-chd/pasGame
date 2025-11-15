using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public int count;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn List (Enemy Types)")]
    public List<EnemySpawnData> spawnList;

    [Header("Spawn Mode")]
    public bool instantSpawn = true;
    public float spawnInterval = 0.5f;

    [Header("Safe Zone (No Enemy Spawn Around Player)")]
    public float safeZoneRadius = 3f;   // musuh tidak boleh spawn dalam radius ini

    private List<Vector2Int> floorPositions;
    private float spawnTimer = 0f;
    private Queue<GameObject> spawnQueue = new Queue<GameObject>();
    private bool isSpawning = false;

    private List<Vector2Int> usedPositions = new List<Vector2Int>();
    private Vector2Int playerPosition = Vector2Int.zero;
    private Vector3 playerWorldPosition;

    private void Update()
    {
        if (instantSpawn || !isSpawning) return;

        if (spawnQueue.Count == 0)
        {
            isSpawning = false;
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnOneEnemy();
            spawnTimer = 0f;
        }
    }

    /// <summary>
    /// Main spawn function
    /// </summary>
    public void SpawnEnemies(HashSet<Vector2Int> floorPosSet, Vector2Int playerPos, int targetCount)
    {
        floorPositions = new List<Vector2Int>(floorPosSet);
        playerPosition = playerPos;

        playerWorldPosition = new Vector3(playerPos.x + 0.5f, playerPos.y + 0.5f, 0);

        // hapus posisi tepat di bawah player
        if (floorPositions.Contains(playerPosition))
            floorPositions.Remove(playerPosition);

        usedPositions.Clear();

        BuildSpawnQueue(targetCount);

        // Spawn boss (juga harus hindari safezone)
        if (DifficultyManager.Instance != null && DifficultyManager.Instance.IsBossWave())
        {
            GameObject bossPrefab = DifficultyManager.Instance.bossEnemyPrefab;
            if (bossPrefab != null)
            {
                Vector2Int bossPos = GetRandomUnusedPosition();
                Vector3 worldPos = new Vector3(bossPos.x + 0.5f, bossPos.y + 0.5f, 0);
                Instantiate(bossPrefab, worldPos, Quaternion.identity);
                usedPositions.Add(bossPos);
                Debug.Log("⚔️ Boss spawned at " + bossPos);
            }
        }

        if (instantSpawn)
            SpawnAllEnemiesInstantly();
        else
            isSpawning = true;
    }

    private void BuildSpawnQueue(int targetCount)
    {
        spawnQueue.Clear();

        if (spawnList == null || spawnList.Count == 0)
        {
            Debug.LogError("❌ Spawn list is empty!");
            return;
        }

        List<GameObject> tempList = new List<GameObject>();
        int totalFromSpawnList = 0;

        foreach (var e in spawnList)
        {
            if (e.enemyPrefab == null) continue;
            totalFromSpawnList += e.count;
        }

        if (totalFromSpawnList < targetCount)
        {
            foreach (var e in spawnList)
            {
                if (e.enemyPrefab == null) continue;
                for (int i = 0; i < e.count; i++)
                    tempList.Add(e.enemyPrefab);
            }

            while (tempList.Count < targetCount)
            {
                int randomIndex = Random.Range(0, spawnList.Count);
                if (spawnList[randomIndex].enemyPrefab != null)
                    tempList.Add(spawnList[randomIndex].enemyPrefab);
            }
        }
        else
        {
            foreach (var e in spawnList)
            {
                if (e.enemyPrefab == null) continue;
                for (int i = 0; i < e.count; i++)
                    tempList.Add(e.enemyPrefab);
            }

            while (tempList.Count > targetCount)
                tempList.RemoveAt(Random.Range(0, tempList.Count));
        }

        // Shuffle
        for (int i = 0; i < tempList.Count; i++)
        {
            int rand = Random.Range(i, tempList.Count);
            (tempList[i], tempList[rand]) = (tempList[rand], tempList[i]);
        }

        foreach (var prefab in tempList)
            if (prefab != null)
                spawnQueue.Enqueue(prefab);
    }

    private void SpawnAllEnemiesInstantly()
    {
        while (spawnQueue.Count > 0 && floorPositions.Count > 0)
        {
            GameObject enemyPrefab = spawnQueue.Dequeue();
            Vector2Int spawnPos = GetRandomUnusedPosition();

            Vector3 worldPos = new Vector3(spawnPos.x + 0.5f, spawnPos.y + 0.5f, 0);
            Instantiate(enemyPrefab, worldPos, Quaternion.identity);

            usedPositions.Add(spawnPos);
        }

        if (UISystem.Instance != null)
            UISystem.Instance.UpdateEnemyCount();
    }

    private void SpawnOneEnemy()
    {
        if (spawnQueue.Count == 0 || floorPositions.Count == 0) return;

        GameObject enemyPrefab = spawnQueue.Dequeue();
        Vector2Int spawnPos = GetRandomUnusedPosition();

        Vector3 worldPos = new Vector3(spawnPos.x + 0.5f, spawnPos.y + 0.5f, 0);
        Instantiate(enemyPrefab, worldPos, Quaternion.identity);

        usedPositions.Add(spawnPos);

        if (UISystem.Instance != null)
            UISystem.Instance.UpdateEnemyCount();
    }

    /// <summary>
    /// Pilih posisi random yang tidak di safezone
    /// </summary>
    private Vector2Int GetRandomUnusedPosition()
    {
        if (floorPositions == null || floorPositions.Count == 0)
            return Vector2Int.zero;

        List<Vector2Int> validPositions = new List<Vector2Int>();

        foreach (var pos in floorPositions)
        {
            if (usedPositions.Contains(pos)) continue;
            if (pos == playerPosition) continue;

            Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);

            // ❌ skip jika dekat player (safezone)
            if (Vector3.Distance(worldPos, playerWorldPosition) < safeZoneRadius)
                continue;

            validPositions.Add(pos);
        }

        // fallback jika semuanya masuk safezone
        if (validPositions.Count == 0)
            return floorPositions[Random.Range(0, floorPositions.Count)];

        return validPositions[Random.Range(0, validPositions.Count)];
    }
}
