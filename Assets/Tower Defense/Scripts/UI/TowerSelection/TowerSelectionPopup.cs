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
}
