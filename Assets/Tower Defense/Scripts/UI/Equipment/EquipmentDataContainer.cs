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
}

public class EquipmentData
{
    public Sprite icon;
    public Rarity rarity;
    public bool isOwned;
    public float attack;
    public float speed;
    public float attackRadius;
}
