using System;
using System.Collections;
using System.Threading.Tasks;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static GameEnum;

public class WinPopup : BasePopup
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private Button x5CoinButton;
    [SerializeField] private RectTransform coinContainer;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedMultiplyCoinText;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;
    [SerializeField] private LevelDataContainer levelDataContainer;

    [Header("CUSTOMIZE")]
    [SerializeField] private float timeCollectCoin;

    private bool _isCollectCoinRemoteConfig;
    private int _multiplierCoinWinLevel;
    private int _currentCollectedCoin;
    private bool _isAlreadyWatchRewardedAd;

    public static event Action<int> goLevelEvent;
    public static event Action nextLevelEvent;
    public static event Action<Vector3, int> collectCoinEvent;

    protected override void RegisterMoreEvent()
    {
        GameStateWin.winLevelEvent += OnLevelWinAsync;
        DebugPopup.winLevelEvent += OnLevelWinAsync;

        continueButton.onClick.AddListener(Continue);
        returnHomeButton.onClick.AddListener(ReturnHome);
        x5CoinButton.onClick.AddListener(X5Coin);
    }

    protected override void UnregisterMoreEvent()
    {
        GameStateWin.winLevelEvent -= OnLevelWinAsync;
        DebugPopup.winLevelEvent -= OnLevelWinAsync;
    }

    private async void OnLevelWinAsync()
    {
        AdmobAdsMax.Instance.ShowBannerCollap();

        coinText.text = $"+{0}";
        continueButton.transform.localScale = Vector3.zero;

        continueButton.interactable = true;
        returnHomeButton.interactable = true;
        x5CoinButton.interactable = true;

        x5CoinButton.transform.localScale = Vector3.one;

        // REMOTE CONFIG IS COLLECT COIN
        _isCollectCoinRemoteConfig = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_COLLECT_COIN_WIN_LEVEL, true);
        _multiplierCoinWinLevel = RemoteConfigController.GetIntConfig(FirebaseConfig.MULTIPLIER_COIN_WIN_LEVEL, 2);

        coinContainer.gameObject.SetActive(_isCollectCoinRemoteConfig);
        x5CoinButton.gameObject.SetActive(_isCollectCoinRemoteConfig);

        if (!_isCollectCoinRemoteConfig)
        {
            continueButton.GetComponent<RectTransform>().localPosition += new Vector3(0, 80, 0);
        }
        else
        {
            localizedMultiplyCoinText.transform.localScale = Vector3.zero;

            _tweens.Add(Tween.Delay(0.2f).OnComplete(() =>
            {
                localizedMultiplyCoinText.UpdateTranslationWithParameter("value", $"{_multiplierCoinWinLevel}");

                _tweens.Add(Tween.Scale(localizedMultiplyCoinText.transform, 1, duration: 0.3f));
            }));
        }
        //

        Show();

        StartCoroutine(LoopScaleUpDownAdsButton());

        SoundManager.Instance.PlaySoundWin();

        await Task.Delay(500);

        _currentCollectedCoin = 0;

        CollectCoin(10);

        TrackLevelWin();

        if (currentLevel.Value - UserData.NumLevelLoop >= levelDataContainer.MaxLevel)
        {
            await UserData.SetNumLevelLoop(UserData.NumLevelLoop + 1);
        }

        currentLevel.Value++;

        currentLevel.Save();

        _isAlreadyWatchRewardedAd = false;
    }

    private void CollectCoin(int numCoin, Action onCompletedAction = null)
    {
        coinText.text = $"+{_currentCollectedCoin}";

        int deltaCoin = (int)(numCoin / 10f);

        _tweens.Add(Tween.Custom(_currentCollectedCoin, numCoin, duration: timeCollectCoin, onValueChange: newVal =>
        {
            coinText.text = $"+{(int)newVal}";
        })
        .OnComplete(() =>
            {
                if (_isCollectCoinRemoteConfig)
                {
                    userResourcesObserver.ChangeCoin(numCoin - _currentCollectedCoin);

                    _currentCollectedCoin += numCoin;
                }

                _tweens.Add(Tween.Scale(continueButton.transform, 1, duration: timeCollectCoin));

                onCompletedAction?.Invoke();
            })
        );
    }

    private async void Continue()
    {
        if (_isAlreadyWatchRewardedAd)
        {
            OnInterstitialAdCompleted();
        }
        else
        {
            continueButton.interactable = false;

            DoubleInterAdsConfig doubleInterAdsConfig = GamePersistentVariable.doubleInterAdsConfig;

            if (doubleInterAdsConfig.IsValidDoubleInter() && currentLevel.Value >= doubleInterAdsConfig.minLevel)
            {
                AdmobAdsMax.Instance.ShowDoubleInterstitialAds
                (
                    OnInterstitialAdCompleted, isShowBreakAds: true
                );
            }
            else
            {
                AdmobAdsMax.Instance.ShowInterstitial(actionIniterClose: OnInterstitialAdCompleted);
            }
        }
    }

    private void OnInterstitialAdCompleted()
    {
        _tweens.Add(Tween.Delay(0.5f).OnComplete(() =>
        {
            HideImmediately();
        }));

        goLevelEvent?.Invoke(currentLevel.Value);

        SaferioTracking.TrackInterstitialAdCompleted(currentLevel.Value, "next_level");

        AdsDurationObserver.duration = 0;
    }

    #region X5 Coin
    private void X5Coin()
    {
        AdmobAdsMax.Instance.ShowVideoReward(
            OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.MultiplyCoinWinLevel);
    }

    private void OnRewaredAdCompleted()
    {
        x5CoinButton.interactable = false;
        continueButton.interactable = false;

        TweenUtil.PunchHide(x5CoinButton.transform);

        continueButton.transform.localScale = Vector3.zero;

        _tweens.Add(Tween.Scale(continueButton.transform, 1.1f, duration: 0.3f)
            .OnComplete(() => _tweens.Add(Tween.Scale(continueButton.transform, 1, duration: 0.15f)
            .OnComplete(() => continueButton.interactable = true)
            ))
        );

        CollectCoin(_multiplierCoinWinLevel * 10);

        SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, "multiply_coin_win_level");

        _isAlreadyWatchRewardedAd = true;
    }

    private void ShowAdsNotLoadedPopup()
    {
        InvokeShowAdNotLoadedPopupEvent();
    }
    #endregion

    #region Button Flow
    private IEnumerator LoopScaleUpDownAdsButton()
    {
        float cycleDuration = 1.2f;

        WaitForSeconds waitForSeconds = new WaitForSeconds(cycleDuration);

        Tween scaleTween = new Tween();

        continueButton.gameObject.SetActive(false);

        Tween.Delay(3).OnComplete(() =>
        {
            TweenUtil.PunchShow(continueButton.transform, Vector3.one);
        });

        while (x5CoinButton.gameObject.activeInHierarchy)
        {
            if (scaleTween.isAlive)
            {
                scaleTween.Stop();
            }

            scaleTween = Tween.Scale(x5CoinButton.transform, 1.1f, duration: cycleDuration / 2f)
            .OnComplete(() =>
            {
                Tween.Scale(x5CoinButton.transform, 1f, duration: cycleDuration / 2f);
            });

            yield return waitForSeconds;
        }
    }
    #endregion

    private void ReturnHome()
    {
        returnHomeButton.interactable = false;

        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }

    private async void TrackLevelWin()
    {
        bool isFirstWinLevel = DataUtility.LoadAsync($"{GameConstants.IS_FIRST_WIN_LEVEL}_{currentLevel.Value}", true);

        if (isFirstWinLevel)
        {
            SaferioTracking.TrackLevelFirstWin(currentLevel.Value, levelBoosterObserver);

            DataUtility.SaveAsync($"{GameConstants.IS_FIRST_WIN_LEVEL}_{currentLevel.Value}", false);
        }

        SaferioTracking.TrackLevelWin(currentLevel.Value, levelBoosterObserver);

        // don't track loop level CostCenter
        if (currentLevel.Value <= levelDataContainer.MaxLevel)
        {
            SaferioTracking.TrackLevelEnd(currentLevel.Value, isSuccess: true, EndLevelReason.Win.ToString());
        }
    }
}
