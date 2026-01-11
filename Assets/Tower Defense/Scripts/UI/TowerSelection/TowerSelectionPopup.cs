using System;
using UnityEngine;

public class TowerSelectionPopup : BasePopup
{
    public TowerSelectionItem[] items;

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
    }

    protected override void Hide()
    {
        Time.timeScale = 1;

        base.Hide();
    }
}
