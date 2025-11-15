using UnityEngine;

public class CollectibleDropSystem : MonoBehaviour
{
    public ItemDatabase itemDatabase;

    [Tooltip("Offset when item drops")]
    public Vector2 dropOffset = new Vector2(0, 0.5f);

    public float scatterRadius = 0.4f;

    public void DropCollectibles()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase missing on enemy!");
            return;
        }

        var droppables = itemDatabase.GetEnemyDroppables();
        if (droppables.Count == 0) return;

        foreach (var entry in droppables)
        {
            float roll = Random.Range(0, 100f);
            if (roll > entry.dropChanceEnemy) continue;

            int amount = Random.Range(entry.minDropAmount, entry.maxDropAmount + 1);

            for (int i = 0; i < amount; i++)
            {
                Vector3 basePos = transform.position + (Vector3)dropOffset;
                Vector3 scatter = Random.insideUnitCircle * scatterRadius;
                Vector3 finalPos = basePos + scatter;

                Instantiate(entry.prefab, finalPos, Quaternion.identity);
            }

            Debug.Log($"Enemy dropped {amount}x {entry.itemName}");
        }
    }
}
