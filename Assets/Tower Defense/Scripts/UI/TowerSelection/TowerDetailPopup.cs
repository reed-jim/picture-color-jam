using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerDetailPopup : BasePopup
{
    public Image icon;
    public TMP_Text damageText;
    public TMP_Text attackRadiusText;
    public TMP_Text rateOfFireText;

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        TowerManager.showTowerDetailPopup += UpdateContent;
    }

    protected override void MoreActionOnDestroy()
    {
        base.MoreActionOnDestroy();

        TowerManager.showTowerDetailPopup -= UpdateContent;
    }

    public void UpdateContent(TowerProperty towerProperty)
    {
        icon.sprite = towerProperty.icon;
        icon.SetNativeSize();

        damageText.text = DisplayStat(towerProperty.damage, towerProperty.baseDamage);
        attackRadiusText.text = DisplayStat(towerProperty.attackRadius, towerProperty.baseAttackRadius);
        rateOfFireText.text = DisplayStat(towerProperty.rateOfFire, towerProperty.baseRateOfFire);

        Show();
    }

    private string DisplayStat(float value, float baseValue)
    {
        return $"<color=#00FF00>{value}</color> ({baseValue})";
    }
}
