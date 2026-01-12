using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerDetailPopup : BasePopup
{
    public Image icon;
    public TMP_Text damageText;
    public TMP_Text attackRadiusText;
    public TMP_Text rateOfFireText;

    public void UpdateContent(TowerProperty towerProperty)
    {
        icon.sprite = towerProperty.icon;
        icon.SetNativeSize();

        damageText.text = $"{towerProperty.damage}";
        attackRadiusText.text = $"{towerProperty.attackRadius}";
        rateOfFireText.text = $"{towerProperty.rateOfFire}";
    }
}
