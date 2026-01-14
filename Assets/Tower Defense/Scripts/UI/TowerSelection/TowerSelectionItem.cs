using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class TowerSelectionItem : MonoBehaviour
{
    public Button selectButton;
    public Image icon;
    public TMP_Text description;

    public UpgradeContainer upgradeContainer;

    public TowerType towerType;
    public bool isBuff;
    public float buffRatio;

    public static Action selectTowerEvent;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void Select()
    {
        if (isBuff)
        {
            TowerManager.Instance.BuffTower(towerType, buffRatio);
        }
        else
        {
            TowerManager.Instance.AddTowerToPool(towerType);
        }

        selectTowerEvent?.Invoke();
    }

    public void UpdateItem(UpgradeOption upgradeOption)
    {
        icon.sprite = upgradeContainer.towerTypeAndIconPairs.First(item => item.towerType == upgradeOption.towerType).icon;
        icon.SetNativeSize();

        if (upgradeOption.isBuff)
        {
            description.text = $"{upgradeOption.towerType} <color=#fff>{upgradeOption.buffRatio * 100}%</color> more powerful";
        }
        else
        {
            description.text = $"{upgradeOption.towerType}";
        }

        towerType = upgradeOption.towerType;
    }
}
