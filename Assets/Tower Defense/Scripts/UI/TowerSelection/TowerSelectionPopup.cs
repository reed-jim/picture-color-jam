using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerSelectionPopup : BasePopup
{
    public TowerSelectionItem[] items;
    public UpgradeContainer upgradeContainer;

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        TowerSelectionItem.selectTowerEvent += Hide;
    }

    protected override void MoreActionOnDestroy()
    {
        base.MoreActionOnDestroy();

        TowerSelectionItem.selectTowerEvent -= Hide;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction: () =>
        {
            Time.timeScale = 0;
        });

        RandomizeUpgrades();
    }

    protected override void Hide()
    {
        Time.timeScale = 1;

        base.Hide();
    }

    public void RandomizeUpgrades()
    {
        List<UpgradeOption> validUpgrades = new List<UpgradeOption>();

        for (int i = 0; i < upgradeContainer.upgradeOptions.Length; i++)
        {
            UpgradeOption upgradeOption = upgradeContainer.upgradeOptions[i];

            if (upgradeOption.isBuff)
            {
                if (TowerManager.Instance.towers.Any(item => item.towerType == upgradeOption.towerType))
                {
                    validUpgrades.Add(upgradeOption);
                }
            }
            else
            {
                validUpgrades.Add(upgradeOption);
            }
        }

        UpgradeOption[] pool = GetRandomItems(validUpgrades.ToArray(), 3);

        for (int i = 0; i < items.Length; i++)
        {
            items[i].UpdateItem(pool[i]);
        }
    }

    public static T[] GetRandomItems<T>(T[] source, int count)
    {
        T[] copy = (T[])source.Clone();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, copy.Length);
            (copy[i], copy[randomIndex]) = (copy[randomIndex], copy[i]);
        }

        return copy.Take(count).ToArray();
    }
}
