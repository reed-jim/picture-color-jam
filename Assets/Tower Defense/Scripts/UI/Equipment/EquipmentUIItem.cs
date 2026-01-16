using UnityEngine;
using UnityEngine.UI;

public class EquipmentUIItem : MonoBehaviour
{
    public Image icon;
    public Image rarityBackground;
    public Image lockedBackground;

    public EquipmentDataContainer equipmentDataContainer;

    public void Lock()
    {
        lockedBackground.gameObject.SetActive(true);
    }

    public void Unlock()
    {
        lockedBackground.gameObject.SetActive(false);
    }

    public void SetData(EquipmentData data)
    {
        icon.sprite = data.icon;
        rarityBackground.sprite = equipmentDataContainer.rarityBackgrounds[(int)data.rarity];
    }

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }

    public void SetRarity(Rarity rarity)
    {
        rarityBackground.sprite = equipmentDataContainer.rarityBackgrounds[(int)rarity];
    }
}
