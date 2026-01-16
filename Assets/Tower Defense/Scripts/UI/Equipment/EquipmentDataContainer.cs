using UnityEngine;

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

public class EquipmentData
{
    public Sprite icon;
    public Rarity rarity;
    public bool isOwned;
    public float damage;
    public float attackSpeed;
    public float attackRadius;
}
