using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static GameEnum;

public class LosePopup : BasePopup
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private Image levelProgressFill;
    [SerializeField] private TMP_Text progressText;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;
    [SerializeField] private LevelObserver levelObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionDuration;

    public static event Action replayLevelEvent;
    public static event Action<int> changeLivesNumberEvent;

    protected override void RegisterMoreEvent()
    {
        // GameStateLose.loseLevelEvent += OnLevelLose;
        GameStateLose.loseLevelEvent += OnLevelLose;
        RevivePopup.reviveRefusedEvent += OnLevelLose;

        replayButton.onClick.AddListener(Replay);
        returnHomeButton.onClick.AddListener(ReturnHome);
    }

    protected override void UnregisterMoreEvent()
    {
        // GameStateLose.loseLevelEvent -= OnLevelLose;
        GameStateLose.loseLevelEvent -= OnLevelLose;
        RevivePopup.reviveRefusedEvent -= OnLevelLose;
    }

    private void OnLevelLose()
    {
        levelProgressFill.fillAmount = 0;

        progressText.text = $"{0}%";

        changeLivesNumberEvent?.Invoke(-1);

        SaferioTracking.TrackLevelLose(currentLevel.Value, levelObserver.Progress, levelBoosterObserver, EndLevelReason.Lose.ToString());

        // don't track loop level CostCenter
        if (currentLevel.Value <= levelDataContainer.MaxLevel)
        {
            SaferioTracking.TrackLevelEnd(currentLevel.Value, isSuccess: false, EndLevelReason.Lose.ToString());
        }

        Show(onCompletedAction: () =>
        {
            Tween.Custom(0, levelObserver.Progress, duration: transitionDuration, onValueChange: newVal =>
            {
                levelProgressFill.fillAmount = newVal;

                progressText.text = $"{(int)(newVal * 100)}%";
            });
        });

        SoundManager.Instance.PlaySoundLose();

        GamePersistentVariable.isLevelLost = true;
    }

    private void Replay()
    {
        if (GamePersistentVariable.livesData.CurrentLives <= 0)
        {
            GamePersistentVariable.isPendingReplay = true;

            SwitchRoute(ScreenRoute.LivesShop);

            return;
        }

        replayLevelEvent?.Invoke();

        Hide();
    }

    private void ReturnHome()
    {
        returnHomeButton.interactable = false;

        Hide(onCompletedAction: () =>
        {
            Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
        });
    }
}
