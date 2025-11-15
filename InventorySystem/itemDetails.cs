using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Image itemImage;

    public void ShowItemDetails(string itemName, string description, Sprite sprite)
    {
        if (nameText != null)
            nameText.text = itemName;

        if (descText != null)
            descText.text = description;

        if (itemImage != null)
        {
            itemImage.sprite = sprite;
            itemImage.enabled = sprite != null;
        }
    }
}
