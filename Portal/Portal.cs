using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Tag of the player")]
    public string playerTag = "Player";

    [Header("Visual Feedback")]
    public GameObject portalVFX;

    [Header("Audio")]
    public AudioClip portalSound;

    private bool hasBeenUsed = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !hasBeenUsed)
        {
            hasBeenUsed = true;
            StartCoroutine(TriggerNextWave(other.gameObject));
        }
    }

    IEnumerator TriggerNextWave(GameObject player)
    {
        Debug.Log("🌀 Portal activated! Collecting items and generating new level...");

        // Play portal sound
        if (audioSource != null && portalSound != null)
        {
            audioSource.PlayOneShot(portalSound);
        }

        // Play portal effect
        if (portalVFX != null)
        {
            Instantiate(portalVFX, transform.position, Quaternion.identity);
        }

        // Give short delay for effect
        yield return new WaitForSeconds(0.5f);

        // --- AUTO COLLECT ALL ITEMS IN SCENE ---
        CollectAllItemsToInventory();

        // Advance difficulty
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.NextLevel();
        }

        // Regenerate level
        LevelGenerator levelGen = FindAnyObjectByType<LevelGenerator>();
        if (levelGen != null)
        {
            levelGen.RegenerateLevel();
        }
        else
        {
            Debug.LogError("LevelGenerator not found!");
        }

        // Destroy the portal
        Destroy(gameObject);
    }

    private void CollectAllItemsToInventory()
    {
        InventoryManager manager = InventoryManager.Instance;
        if (manager == null)
        {
            Debug.LogError("InventoryManager not found! Cannot collect items.");
            return;
        }

        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        int collectedCount = 0;

        foreach (GameObject itemObj in collectibles)
        {
            if (itemObj == null) continue;

            CollectibleItem collectible = itemObj.GetComponent<CollectibleItem>();
            if (collectible != null)
            {
                string name = collectible.GetItemName();
                int qty = collectible.GetQuantity();
                Sprite sprite = collectible.GetItemSprite();
                string description = collectible.GetItemDescription();

                manager.addItem(name, qty, sprite, description);

                Destroy(itemObj);
                collectedCount++;
            }
        }

        Debug.Log($"✅ Collected {collectedCount} items automatically.");
    }
}
