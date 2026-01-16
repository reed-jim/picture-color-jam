using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDetailPopup : BasePopup
{
    public Image icon;
    public TMP_Text damageText;
    public TMP_Text attackSpeedText;
    public TMP_Text attackRadiusText;

    public void UpdateContent(EquipmentData equipmentData)
    {
        icon.sprite = equipmentData.icon;
        damageText.text = $"{equipmentData.damage}";
        attackSpeedText.text = $"{equipmentData.attackSpeed}";
        damageText.text = $"{equipmentData.attackRadius}";
    }
}
