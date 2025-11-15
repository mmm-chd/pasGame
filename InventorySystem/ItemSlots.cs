using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class ItemSlots : MonoBehaviour, IPointerClickHandler
{
    public Image itemImage;
    public TMP_Text itemQuantityText;
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public GameObject selectedPanel;

    private bool isSelected = false;

    private static ItemSlots lastClickedSlot = null;
    private static float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;

    public void addItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        if (!isFull)
        {
            this.itemName = itemName;
            this.itemSprite = itemSprite;
            this.quantity = 0;
            this.itemDescription = itemDescription;
        }

        this.quantity += quantity;
        itemImage.sprite = itemSprite;
        itemImage.enabled = true;
        itemQuantityText.text = this.quantity.ToString();
        isFull = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFull || string.IsNullOrEmpty(itemName)) return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        float now = Time.time;

        // Jika slot berbeda → reset double click state
        if (lastClickedSlot != this)
        {
            lastClickedSlot = this;
            lastClickTime = now;
            OnLeftClick();
            return;
        }

        // Slot sama → cek apakah double click
        if (now - lastClickTime <= doubleClickThreshold)
        {
            UseItem();
            lastClickedSlot = null; // reset setelah dipakai
            return;
        }

        // Klik pertama pada slot yang sama (bukan double click)
        lastClickTime = now;
        OnLeftClick();
    }

    public void OnLeftClick()
    {
        InventoryManager.Instance.SelectSlot(this);
    }

    public void Deselect()
    {
        isSelected = false;
        if (selectedPanel != null)
            selectedPanel.SetActive(false);
    }

    public void Select()
    {
        isSelected = true;
        if (selectedPanel != null)
            selectedPanel.SetActive(true);

        ItemDetailsUI detailsUI = FindAnyObjectByType<ItemDetailsUI>();
        if (detailsUI != null)
            detailsUI.ShowItemDetails(itemName, itemDescription, itemSprite);
    }

    private void UseItem()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        switch (itemName)
        {
            case "Eyeon":
                player.UpgradeRange(0.5f);
                Debug.Log("Range Potion used! New range: " + player.blackholeRangeMultiplier);
                break;

            case "Gelloo":
                player.Heal(5);
                Debug.Log("Heal Potion used! Health: " + player.healthSystem.currentHealth);
                break;
        }

        quantity--;

        if (quantity <= 0)
        {
            InventoryManager.Instance.RemoveSlot(this);
            Destroy(gameObject);
        }
        else
        {
            itemQuantityText.text = quantity.ToString();
        }
    }
}
