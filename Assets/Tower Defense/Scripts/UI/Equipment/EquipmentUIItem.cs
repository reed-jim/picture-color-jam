using System;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUIItem : MonoBehaviour
{
    public Button showDetailButton;
    public Image icon;
    public Image rarityBackground;
    public Image lockedBackground;

    public EquipmentDataContainer equipmentDataContainer;

    private EquipmentData equipmentData;

    public static event Action<EquipmentData> showEquipmentDetailEvent;

    void Awake()
    {
        showDetailButton.onClick.AddListener(ShowEquipmentDetail);
    }

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

        equipmentData = data;
    }

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }

    public void SetRarity(Rarity rarity)
    {
        rarityBackground.sprite = equipmentDataContainer.rarityBackgrounds[(int)rarity];
    }

    public void ShowEquipmentDetail()
    {
        showEquipmentDetailEvent?.Invoke(equipmentData);
    }
}
