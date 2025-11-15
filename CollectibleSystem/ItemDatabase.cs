using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [System.Serializable]
    public class ItemEntry
    {
        public string itemName;
        public GameObject prefab;
        public ItemRarity rarity;

        [Header("Drop Settings")]
        [Range(0, 100)]
        public float dropChanceEnemy = 50f;

        [Header("Spawn Settings (on map)")]
        public bool allowDungeonSpawn = true;
        public float spawnWeight = 50f;

        [Header("Amount")]
        public int minDropAmount = 1;
        public int maxDropAmount = 1;
    }

    public List<ItemEntry> items = new List<ItemEntry>();

    public List<ItemEntry> GetDungeonSpawnables()
    {
        return items.FindAll(i => i.allowDungeonSpawn && i.prefab != null);
    }

    public List<ItemEntry> GetEnemyDroppables()
    {
        return items.FindAll(i => i.dropChanceEnemy > 0 && i.prefab != null);
    }
}
