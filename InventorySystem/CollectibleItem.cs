using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private int quantity = 1;
    [TextArea]
    [SerializeField] private string itemDescription;

    private InventoryManager manager;

    void Start()
    {
        manager = FindAnyObjectByType<InventoryManager>();
        if (manager == null)
            Debug.LogError("InventoryManager not found!");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            manager.addItem(itemName, quantity, itemSprite, itemDescription);
            Destroy(gameObject);
        }
    }

    public void Collect()
    {
        if (manager != null)
            manager.addItem(itemName, quantity, itemSprite, itemDescription);
    }

    // Getter
    public string GetItemName() => itemName;
    public int GetQuantity() => quantity;
    public Sprite GetItemSprite() => itemSprite;
    public string GetItemDescription() => itemDescription;
}
