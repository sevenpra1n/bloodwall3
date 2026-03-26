using UnityEngine;

[System.Serializable]
public class Equipment
{
    public string equipmentName;
    public string description;
    public int power;
    public Sprite icon;
    public EquipmentType type;
    public Rarity rarity;
    public int price;
    public bool isPurchased = false;
}

[System.Serializable]
public enum EquipmentType
{
    Weapon,
    Head,
    Body,
    Legs
}

[System.Serializable]
public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary,
    Mythical
}