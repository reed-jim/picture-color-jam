using System;
using UnityEngine;

public enum EquipmentType
{
    Weapon,
    Helmet,
    Accessory,
    Offhand
}

public enum Rarity
{
    Common,
    Rare,
    Unique,
    Elite,
    Mystery,
    Epic,
    Legendary,
    Divine
}

[CreateAssetMenu(fileName = "EquipmentDataContainer", menuName = "Scriptable Objects/EquipmentDataContainer")]
public class EquipmentDataContainer : ScriptableObject
{
    public EquipmentData[] items;
    public Sprite[] rarityBackgrounds;
}

[Serializable]
public class EquipmentData
{
    public string id => icon.name;
    public Sprite icon;
    public EquipmentType equipmentType;
    public Rarity rarity;
    public bool isOwned;
    public bool isEquiped;
    public float damage;
    public float attackSpeed;
    public float attackRadius;
}

public class CharacterEquipmentData
{
    public CharacterEquipmentSlot[] characterEquipmentSlots;
}

public class CharacterEquipmentSlot
{
    public EquipmentType equipmentType;
    public string equipmentId;
}
