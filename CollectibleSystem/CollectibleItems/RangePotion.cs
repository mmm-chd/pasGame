using UnityEngine;

public class RangePotion : MonoBehaviour
{
    public string itemName = "Range Potion";
    public float bonus = 0.5f;
    public Sprite itemSprite;
    public string itemDescription = "Increases blackhole range";

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            InventoryManager.Instance.addItem(itemName, 1, itemSprite, itemDescription);
            Destroy(gameObject);
        }
    }
}
