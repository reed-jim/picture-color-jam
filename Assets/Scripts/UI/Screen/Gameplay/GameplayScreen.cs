using System;
using System.Threading.Tasks;
using Lean.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class GameplayScreen : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button replayButton;
    // [SerializeField] private Button openRemoveAdsPopupButton;
    [SerializeField] private Button openDebugPopupButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedLevelText;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private GameSetting gameSetting;

    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action nextLevelEvent;
    public static event Action prevLevelEvent;
    public static event Action replayLevelEvent;

    void Awake()
    {
        localizedLevelText.textTranslatedEvent += OnLevelTextTranslated;
        LevelLoader.setLevelNumberEvent += SetLevelText;

        pauseButton.onClick.AddListener(Pause);
        replayButton.onClick.AddListener(Replay);
        // openRemoveAdsPopupButton.onClick.AddListener(OpenRemoveAdsPopupButton);
        openDebugPopupButton.onClick.AddListener(OpenDebugPopupButton);

        // if (UserData.IsRemoveAds)
        // {
        //     openRemoveAdsPopupButton.gameObject.SetActive(false);
        // }

        if (!gameSetting.IsDebug)
        {
            openDebugPopupButton.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        localizedLevelText.textTranslatedEvent -= OnLevelTextTranslated;
        LevelLoader.setLevelNumberEvent -= SetLevelText;
    }

    private async void Pause()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Pause);
    }

    private async void Replay()
    {
        replayLevelEvent?.Invoke();
    }

    private void OpenRemoveAdsPopupButton()
    {
        switchRouteEvent?.Invoke(ScreenRoute.RemoveAd);
    }

    private void OpenDebugPopupButton()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Debug);
    }

    private void OnLevelTextTranslated()
    {
        localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{GetDisplayedLevel()}");
    }

    private void SetLevelText()
    {
        localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{GetDisplayedLevel()}");
    }

    private int GetDisplayedLevel()
    {
        return currentLevel.Value;
    }
}
