using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public InputAction OpenInventory;
    public GameObject InventoryMenu;
    private bool menuActivated;
    public bool IsInventoryOpen => menuActivated;

    private ItemSlots currentlySelectedSlot;

    public GameObject itemSlotPrefab;
    public Transform inventorySlotsParent;

    private List<ItemSlots> itemSlots = new List<ItemSlots>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        OpenInventory = new InputAction("OpenInventory", binding: "<Keyboard>/I");
    }

    void OnEnable()
    {
        OpenInventory.Enable();
        OpenInventory.performed += ToggleInventory;
    }

    void OnDisable()
    {
        OpenInventory.performed -= ToggleInventory;
        OpenInventory.Disable();
    }

    private void ToggleInventory(InputAction.CallbackContext context)
    {
        menuActivated = !menuActivated;
        InventoryMenu.SetActive(menuActivated);

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null) player.canAttack = !menuActivated;

        Time.timeScale = menuActivated ? 0f : 1f;

        // ⬇️ Saat inventory dibuka → pilih item pertama
        if (menuActivated)
            AutoSelectFirstItem();
    }

    // ===========================================
    // 🔥 AUTO SELECT FIRST SLOT (kiri atas)
    // ===========================================
    private void AutoSelectFirstItem()
    {
        if (itemSlots.Count == 0) return;

        SelectSlot(itemSlots[0]);   // slot paling awal = pojok kiri atas
    }

    public void addItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        foreach (var slot in itemSlots)
        {
            if (slot.itemName == itemName)
            {
                slot.addItem(itemName, quantity, itemSprite, itemDescription);
                return;
            }
        }

        GameObject newSlotObj = Instantiate(itemSlotPrefab, inventorySlotsParent);
        ItemSlots newSlot = newSlotObj.GetComponent<ItemSlots>();
        newSlot.addItem(itemName, quantity, itemSprite, itemDescription);

        itemSlots.Add(newSlot);

        // Jika ini item pertama, langsung select
        if (itemSlots.Count == 1)
            SelectSlot(newSlot);
    }

    // ===========================================
    // 🔥 HANYA BOLEH 1 SELECTED SLOT
    // ===========================================
    public void SelectSlot(ItemSlots newSlot)
    {
        if (newSlot == null) return;                  // safety
        if (newSlot.gameObject == null) return;       // safety untuk destroyed object

        if (currentlySelectedSlot != null && currentlySelectedSlot != newSlot)
            currentlySelectedSlot.Deselect();

        currentlySelectedSlot = newSlot;
        newSlot.Select();
    }


    public void RemoveSlot(ItemSlots slot)
    {
        if (itemSlots.Contains(slot))
            itemSlots.Remove(slot);

        // Jika slot yang dihapus adalah yang sedang terselect
        if (currentlySelectedSlot == slot)
        {
            currentlySelectedSlot = null;

            // Pilih ulang slot pertama jika masih ada item
            if (itemSlots.Count > 0)
                SelectSlot(itemSlots[0]);
        }
    }
}
