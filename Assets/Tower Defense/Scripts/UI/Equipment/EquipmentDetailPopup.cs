using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDetailPopup : BasePopup
{
    public Image icon;
    public TMP_Text damageText;
    public TMP_Text attackSpeedText;
    public TMP_Text attackRadiusText;
    public Button equipButton;
    public TMP_Text equipButtonText;

    private EquipmentData equipmentData;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        EquipmentUIItem.showEquipmentDetailEvent += ShowEquipmentDetail;

        equipButton.onClick.AddListener(Equip);
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        EquipmentUIItem.showEquipmentDetailEvent -= ShowEquipmentDetail;
    }

    private void ShowEquipmentDetail(EquipmentData equipmentData)
    {
        Show();

        this.equipmentData = equipmentData;

        UpdateContent(equipmentData);
    }

    public void UpdateContent(EquipmentData equipmentData)
    {
        icon.sprite = equipmentData.icon;
        icon.SetNativeSize();
        icon.rectTransform.sizeDelta *= 2;
        damageText.text = $"{equipmentData.damage}";
        attackSpeedText.text = $"{equipmentData.attackSpeed:F2}";
        attackRadiusText.text = $"{equipmentData.attackRadius:F2}";

        equipButton.interactable = equipmentData.isOwned;
        equipButtonText.text = equipmentData.isEquiped ? "Unequip" : "Equip";

        if (!equipmentData.isOwned)
        {
            equipButtonText.text = "Not Owned";
        }
    }

    public void Equip()
    {
        if (equipmentData.isEquiped)
        {
            EquipmentScreen.Instance.Unequip(equipmentData);
        }
        else
        {
            EquipmentScreen.Instance.Equip(equipmentData);
        }

        Hide();
    }
}
