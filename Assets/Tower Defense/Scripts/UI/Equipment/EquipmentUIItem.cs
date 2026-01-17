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
        TweenUtil.PunchShow(icon.transform, Vector3.one);

        icon.sprite = data.icon;
        icon.SetNativeSize();
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
        if (equipmentData != null)
        {
            showEquipmentDetailEvent?.Invoke(equipmentData);
        }
    }

    public void MarkEmpty()
    {
        TweenUtil.PunchHide(icon.transform);

        equipmentData = null;
    }
}
