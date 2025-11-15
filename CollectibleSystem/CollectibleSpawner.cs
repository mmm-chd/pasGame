using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CollectibleSpawner : MonoBehaviour
{
    [Header("Database")]
    public ItemDatabase itemDatabase;

    [Header("Difficulty Settings")]
    public int collectibleCount = 5;

    private List<ItemDatabase.ItemEntry> spawnableItems;

    public void SpawnCollectibles(HashSet<Vector2Int> floorPositions, Vector2Int playerStart)
    {
        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase not assigned!");
            return;
        }

        spawnableItems = itemDatabase.GetDungeonSpawnables();
        if (spawnableItems.Count == 0)
        {
            Debug.LogWarning("No spawnable collectibles found in ItemDatabase!");
            return;
        }

        List<Vector2Int> points = floorPositions.ToList();
        points.Remove(playerStart);

        if (points.Count == 0) return;
        if (collectibleCount > points.Count)
            collectibleCount = points.Count;

        for (int i = 0; i < collectibleCount; i++)
        {
            ItemDatabase.ItemEntry item = SelectItemByWeight();
            if (item == null) continue;

            int index = Random.Range(0, points.Count);
            Vector2Int pos = points[index];
            points.RemoveAt(index);

            Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);

            Instantiate(item.prefab, worldPos, Quaternion.identity);
        }
    }

    private ItemDatabase.ItemEntry SelectItemByWeight()
    {
        float total = spawnableItems.Sum(i => i.spawnWeight);
        float roll = Random.Range(0, total);

        float current = 0;
        foreach (var item in spawnableItems)
        {
            current += item.spawnWeight;
            if (roll <= current)
                return item;
        }

        return spawnableItems[0];
    }
}
