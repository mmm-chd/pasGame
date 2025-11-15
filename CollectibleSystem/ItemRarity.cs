using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[System.Serializable]
public class ItemRarityData
{
    public ItemRarity rarity;
    public float weight = 50f;  // Semakin besar semakin umum
}
