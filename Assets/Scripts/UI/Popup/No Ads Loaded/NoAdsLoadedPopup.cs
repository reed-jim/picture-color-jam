using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class NoAdsLoadedPopup : BasePopup
{
    [SerializeField] private Button okButton;

    private ScreenRoute _returnRoute = ScreenRoute.None;

    public static event Action<ScreenRoute> switchRouteEvent;

    protected override void RegisterMoreEvent()
    {
        RevivePopup.showNoInternetPopup += ShowWithReturnRoute;
        showAdNotLoadedPopupEvent += Show;

        okButton.onClick.AddListener(Hide);
    }

    protected override void UnregisterMoreEvent()
    {
        RevivePopup.showNoInternetPopup -= ShowWithReturnRoute;
        showAdNotLoadedPopupEvent -= Show;
    }

    protected override void Hide()
    {
        base.Hide();

        _tweens.Add(Tween.Delay(0.8f).OnComplete(() =>
        {
            if (_returnRoute != ScreenRoute.None)
            {
                switchRouteEvent?.Invoke(_returnRoute);

                _returnRoute = ScreenRoute.None;
            }
        }));
    }

    private void ShowWithReturnRoute(ScreenRoute returnRoute)
    {
        switchRouteEvent(ScreenRoute.NoInternet);

        _returnRoute = returnRoute;
    }
}
