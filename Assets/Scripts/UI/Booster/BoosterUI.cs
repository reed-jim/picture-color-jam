using System;
using System.Collections.Generic;
using Lean.Localization;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;
using static GameEnum;

public class BoosterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform boosterContainer;
    [SerializeField] private RectTransform addMoreScrewPortButtonRT;
    [SerializeField] private RectTransform breakModeButtonRT;
    [SerializeField] private RectTransform clearAllScrewPortsButtonRT;
    [SerializeField] private RectTransform breakModeContainer;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedBreakModeDescription;
    [SerializeField] private Image freezedBackground;

    [SerializeField] private Button addMoreScrewPortButton;
    [SerializeField] private Button breakObjectButton;
    [SerializeField] private Button clearAllScrewPortsButton;
    [SerializeField] private Button useTeleportBoosterButton;
    [SerializeField] private Button disableBreakObjectModeEarlyButton;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private Vector2Variable canvasSize;
    [SerializeField] private Vector2Variable canvasSizeOfReferenceDevice;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionTime;

    public static event Action addMoreScrewPortEvent;
    public static event Action enableBreakObjectModeEvent;
    public static event Action disableBreakObjectModeEvent;
    public static event Action<bool> enableVacumnModeEvent;
    public static event Action enableTeleportModeEvent;
    public static event Action clearAllScrewPortsEvent;
    public static event Action<BoosterType> showBuyBoosterPopupEvent;
    public static event Action<int> updateBoosterQuantityEvent;
    public static event Action shakeScrewPortsEvent;
    public static event Action<string, string> showNotificationEvent;

    private List<Tween> _tweens;
    private int _numScrewPortsAdded;
    private bool _isInTransition;
    private bool _isEnable;
    private bool _isPreventShowWarningPopup;
    private bool _isTimeFreezing;

    private Vector2 _initialBoosterContainerPosition;
    private Vector2 _initialAddMoreScrewPortButtonPosition;
    private Vector2 _initialbreakObjectButtonPosition;
    private Vector2 _initialClearAllScrewPortsButtonPosition;

    public static event Action freezeTimeEvent;

    private void Awake()
    {
        LevelLoader.startLevelEvent += Reset;
        BuyBoosterPopup.useBoosterEvent += UseBooster;
        BoosterTutorial.useBoosterType += UseBooster;
        ScrewBoxManager.enableBoosterEvent += EnableBooster;
        ScrewManager.enableBoosterEvent += EnableBoosterWithoutWarningPopup;
        RevivePopup.reviveEvent += OnRevived;
        BlockSelectionInput.breakObjectEvent += ConfirmBreakObject;
        BlockSelectionInput.vacumnEvent += ConfirmVacumnBooster;
        BaseBlock.useTeleportBoosterEvent += ConfirmTeleportBooster;
        LevelTimeCounter.unfreezeTimeEvent += UnfreezeTime;
        LevelTimeCounter.compensateUnfreezeBoosterEvent += CompensateUnfreezeBooster;
        BlockSelectionInput.startVacumnEvent += HideUIWhileMagneting;

        addMoreScrewPortButton.onClick.AddListener(() => UseBooster(BoosterType.FreezeTime));
        breakObjectButton.onClick.AddListener(() => UseBooster(BoosterType.BreakObject));
        clearAllScrewPortsButton.onClick.AddListener(() => UseBooster(BoosterType.Vacumn));
        useTeleportBoosterButton.onClick.AddListener(() => UseBooster(BoosterType.Teleport));
        disableBreakObjectModeEarlyButton.onClick.AddListener(DisableBreakObjectModeEarly);

        _tweens = new List<Tween>();

        _isEnable = true;
    }

    private void Start()
    {
        _initialBoosterContainerPosition = boosterContainer.localPosition;
        _initialAddMoreScrewPortButtonPosition = addMoreScrewPortButtonRT.localPosition;
        _initialbreakObjectButtonPosition = breakModeButtonRT.localPosition;
        _initialClearAllScrewPortsButtonPosition = clearAllScrewPortsButtonRT.localPosition;
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= Reset;
        BuyBoosterPopup.useBoosterEvent -= UseBooster;
        BoosterTutorial.useBoosterType -= UseBooster;
        ScrewBoxManager.enableBoosterEvent -= EnableBooster;
        ScrewManager.enableBoosterEvent -= EnableBoosterWithoutWarningPopup;
        RevivePopup.reviveEvent -= OnRevived;
        BlockSelectionInput.breakObjectEvent -= ConfirmBreakObject;
        BlockSelectionInput.vacumnEvent -= ConfirmVacumnBooster;
        BaseBlock.useTeleportBoosterEvent -= ConfirmTeleportBooster;
        LevelTimeCounter.unfreezeTimeEvent -= UnfreezeTime;
        LevelTimeCounter.compensateUnfreezeBoosterEvent -= CompensateUnfreezeBooster;
        BlockSelectionInput.startVacumnEvent -= HideUIWhileMagneting;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void Reset()
    {
        _tweens.Add(Tween.LocalPosition(boosterContainer, _initialBoosterContainerPosition, duration: transitionTime));
        _tweens.Add(Tween.LocalPosition(addMoreScrewPortButtonRT, _initialAddMoreScrewPortButtonPosition, duration: transitionTime));
        _tweens.Add(Tween.LocalPosition(breakModeButtonRT, _initialbreakObjectButtonPosition, duration: transitionTime));
        _tweens.Add(Tween.LocalPosition(clearAllScrewPortsButtonRT, _initialClearAllScrewPortsButtonPosition, duration: transitionTime));

        breakModeContainer.gameObject.SetActive(false);

        addMoreScrewPortButton.interactable = true;

        _numScrewPortsAdded = 0;
        _isInTransition = false;
        _isEnable = true;
        _isPreventShowWarningPopup = false;
        _isTimeFreezing = false;
    }

    private void OnRevived(BoosterType boosterType)
    {
        _isEnable = false;
        _isPreventShowWarningPopup = false;

        _tweens.Add(Tween.Delay(2f).OnComplete(() =>
        {
            _isEnable = true;
            _isPreventShowWarningPopup = true;
        }));
    }

    private void UseBooster(BoosterType boosterType)
    {
        if (!_isEnable)
        {
            if (!_isPreventShowWarningPopup)
            {
                showNotificationEvent?.Invoke(GameConstants.WARNING, GameConstants.TOO_FAST_ACTION);
            }

            return;
        }

        if (boosterType == BoosterType.FreezeTime)
        {
            FreezeTime();
        }
        else if (boosterType == BoosterType.BreakObject)
        {
            EnableBreakObjectMode();
        }
        else if (boosterType == BoosterType.Vacumn)
        {
            EnableVacumnMode();
        }
        else if (boosterType == BoosterType.Teleport)
        {
            EnableTeleportMode();
        }
    }

    private void FreezeTime()
    {
        if (_isTimeFreezing)
        {
            showNotificationEvent?.Invoke(GameConstants.WARNING, GameConstants.BOOSTER_IN_USE);

            return;
        }

        if (!IsBoosterAvailable(boosterIndex: 0))
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.FreezeTime);

            return;
        }

        freezeTimeEvent?.Invoke();

        ConsumeBooster(0);

        _isTimeFreezing = true;
    }

    private void UnfreezeTime()
    {
        _isTimeFreezing = false;
    }

    private void CompensateUnfreezeBooster()
    {
        userResourcesObserver.ChangeBoosterQuantity(0, 1);

        updateBoosterQuantityEvent?.Invoke(0);
    }

    private void EnableBreakObjectMode()
    {
        if (!IsBoosterAvailable(boosterIndex: 1))
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.BreakObject);

            return;
        }

        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        breakModeContainer.gameObject.SetActive(true);

        localizedBreakModeDescription.TranslationName = "Select a block to break";

        _tweens.Add(Tween.LocalPositionY(boosterContainer, -canvasSizeOfReferenceDevice.Value.y, duration: transitionTime));
        _tweens.Add(Tween.LocalPositionY(breakModeContainer, _initialBoosterContainerPosition.y + 0.0f * breakModeContainer.sizeDelta.y, duration: transitionTime)
        .OnComplete(() =>
        {
            enableBreakObjectModeEvent?.Invoke();

            _isInTransition = false;
        }));
    }

    private void ConfirmBreakObject()
    {
        if (GamePersistentVariable.isUseHammerToBreak1x1Tile)
        {
            GamePersistentVariable.isUseHammerToBreak1x1Tile = false;
        }
        else
        {
            ConsumeBooster(boosterIndex: (int)BoosterType.BreakObject);
        }

        DisableBreakObjectMode();
    }

    private void DisableBreakObjectMode()
    {
        _tweens.Add(Tween.LocalPositionY(boosterContainer, _initialBoosterContainerPosition.y, duration: transitionTime));
        _tweens.Add(Tween.LocalPositionY(breakModeContainer, -canvasSizeOfReferenceDevice.Value.y, duration: transitionTime)
        .OnComplete(() =>
        {
            breakModeContainer.gameObject.SetActive(false);
        }));
    }

    private void EnableVacumnMode()
    {
        if (!IsBoosterAvailable(boosterIndex: 2))
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.Vacumn);

            return;
        }

        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        breakModeContainer.gameObject.SetActive(true);

        localizedBreakModeDescription.TranslationName = "Completes a specific picture";

        _tweens.Add(Tween.LocalPositionY(boosterContainer, -canvasSizeOfReferenceDevice.Value.y, duration: transitionTime));
        _tweens.Add(Tween.LocalPositionY(breakModeContainer, _initialBoosterContainerPosition.y + 0.0f * breakModeContainer.sizeDelta.y, duration: transitionTime)
        .OnComplete(() =>
        {
            enableVacumnModeEvent?.Invoke(true);

            _isInTransition = false;
        }));
    }

    private void EnableTeleportMode()
    {
        if (!IsBoosterAvailable(boosterIndex: 3))
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.Teleport);

            return;
        }

        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        breakModeContainer.gameObject.SetActive(true);

        localizedBreakModeDescription.TranslationName = "Move a block to any free space";

        _tweens.Add(Tween.LocalPositionY(boosterContainer, -canvasSizeOfReferenceDevice.Value.y, duration: transitionTime));
        _tweens.Add(Tween.LocalPositionY(breakModeContainer, _initialBoosterContainerPosition.y + 0.0f * breakModeContainer.sizeDelta.y, duration: transitionTime)
        .OnComplete(() =>
        {
            enableTeleportModeEvent?.Invoke();
            
            _isInTransition = false;
        }));
    }

    private void HideUIWhileMagneting()
    {
        canvasGroup.interactable = false;

        _tweens.Add(Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
        {
            canvasGroup.alpha = newVal;
        }));

        _tweens.Add(Tween.Delay(3).OnComplete(() =>
        {
            ShowUIAfterMagneting();
        }));
    }

    private void ShowUIAfterMagneting()
    {
        _tweens.Add(Tween.Custom(0, 1, duration: 0.3f, onValueChange: newVal =>
        {
            canvasGroup.alpha = newVal;

            canvasGroup.interactable = true;
        }));
    }

    private void ConfirmVacumnBooster(string spriteType)
    {
        ConsumeBooster(boosterIndex: (int)BoosterType.Vacumn);

        DisableVacumnMode();
    }

    private void DisableVacumnMode()
    {
        _tweens.Add(Tween.LocalPositionY(boosterContainer, _initialBoosterContainerPosition.y, duration: transitionTime));
        _tweens.Add(Tween.LocalPositionY(breakModeContainer, -canvasSizeOfReferenceDevice.Value.y, duration: transitionTime)
        .OnComplete(() =>
        {
            breakModeContainer.gameObject.SetActive(false);
        }));
    }

    private void ConfirmTeleportBooster()
    {
        ConsumeBooster(boosterIndex: (int)BoosterType.Teleport);

        DisableBreakObjectMode();
    }

    private void DisableBreakObjectModeEarly()
    {
        DisableBreakObjectMode();

        disableBreakObjectModeEvent?.Invoke();
    }

    private void EnableBooster(bool isEnable)
    {
        _isEnable = isEnable;
    }

    private void EnableBoosterWithoutWarningPopup(bool isEnable)
    {
        _isEnable = isEnable;
        _isPreventShowWarningPopup = true;
    }

    private bool IsBoosterAvailable(int boosterIndex)
    {
        userResourcesObserver.Load();

        if (userResourcesObserver.UserResources.BoosterQuantities[boosterIndex] > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ConsumeBooster(int boosterIndex)
    {
        userResourcesObserver.ConsumeBooster(boosterIndex: boosterIndex);
        levelBoosterObserver.UseBooster(boosterIndex);

        updateBoosterQuantityEvent?.Invoke(boosterIndex);

        SaferioTracking.TrackBoosterUsage((BoosterType)boosterIndex);
    }
}
