using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class LevelTimeCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Image freezedBackground;

    [SerializeField] private CanvasGroup freezeProgressGroup;
    [SerializeField] private Image freezeProgressFill;
    [SerializeField] private RectTransform movingFreezeClock;
    [SerializeField] private Image movingFreezeClockImage;

    [SerializeField] private ParticleSystem snowFx;
    [SerializeField] private ParticleSystem breakClockFx;

    [SerializeField] private int totalSecond;

    [SerializeField] private IntVariable currentLevel;

    private List<Tween> _tweens;
    private List<Sequence> _sequences;
    private Coroutine _countingCoroutine;
    private bool _isFreeze;
    private bool _isPaused;
    private Vector3 _initialMovingFreezeClockPosition;
    private int _remainingSecond;

    public static event Action loseLevelEvent;
    public static event Action unfreezeTimeEvent;
    public static event Action<string, string> showNotificationEvent;
    public static event Action compensateUnfreezeBoosterEvent;

    // TEMP
    private int[] levelsTime = new int[50] {
        300, 300, 300, 300, 300, 300, 300, 300, 300, 300,
        200, 100, 150, 120, 100, 120, 120, 120, 100, 140,
        100, 120, 100, 150, 85, 100, 130, 120, 170, 240,
        100, 160, 80, 120, 240, 180, 115, 120, 180, 240,
        180, 150, 120, 160, 195, 120, 120, 200, 200, 200
    };

    private void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelStarted;
        GameStateWin.winLevelEvent += OnLevelWon;
        BoosterUI.freezeTimeEvent += FreezeTime;
        RevivePopup.reviveEvent += Revive;
        PausePopup.pauseEvent += Pause;
        IAPShopPopup.pauseEvent += Pause;
        DebugPopup.loseLevelEvent += DebugLose;

        _tweens = new List<Tween>();
        _sequences = new List<Sequence>();

        _initialMovingFreezeClockPosition = movingFreezeClock.localPosition;
    }

    private void Start()
    {
        GamePersistentVariable.reviveAdditionalTime = RemoteConfigController.GetIntConfig(FirebaseConfig.REVIVE_ADDITIONAL_TIME, 20);
    }

    private void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
        GameStateWin.winLevelEvent -= OnLevelWon;
        BoosterUI.freezeTimeEvent -= FreezeTime;
        RevivePopup.reviveEvent -= Revive;
        PausePopup.pauseEvent -= Pause;
        IAPShopPopup.pauseEvent -= Pause;
        DebugPopup.loseLevelEvent -= DebugLose;

        CommonUtil.StopAllTweens(_tweens);
        CommonUtil.StopAllSequences(_sequences);
    }

    private void OnLevelStarted()
    {
        if (_countingCoroutine != null)
        {
            StopCoroutine(_countingCoroutine);
        }

        totalSecond = levelsTime[currentLevel.Value % 50];

        if (currentLevel.Value >= 50)
        {
            totalSecond = 200 + 10 * currentLevel.Value % 5;
        }

        _countingCoroutine = StartCoroutine(Counting());

        if (_isFreeze)
        {
            Unfreeze();
        }

        freezeProgressGroup.gameObject.SetActive(false);
    }

    private void OnLevelWon()
    {
        if (_countingCoroutine != null)
        {
            StopCoroutine(_countingCoroutine);
        }
    }

    private IEnumerator Counting()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        int remainingSecond = totalSecond;

        timeText.text = ConvertSecondsToMinutesSeconds(remainingSecond);

        while (remainingSecond >= 0)
        {
            if (!_isFreeze && !_isPaused && GamePersistentVariable.isLevelDirty)
            {
                timeText.text = ConvertSecondsToMinutesSeconds(remainingSecond);

                remainingSecond--;
            }

            _remainingSecond = remainingSecond;

            yield return waitForSeconds;
        }

        yield return new WaitForSeconds(2);

        loseLevelEvent?.Invoke();
    }

    private void FreezeTime()
    {
        if (_remainingSecond <= 0)
        {
            showNotificationEvent?.Invoke("Warning", "You have run out of time");

            compensateUnfreezeBoosterEvent?.Invoke();
            unfreezeTimeEvent?.Invoke();

            return;
        }

        _isFreeze = true;

        freezeProgressGroup.gameObject.SetActive(true);

        freezeProgressGroup.alpha = 0;
        freezeProgressFill.fillAmount = 1;

        // clock item
        movingFreezeClock.gameObject.SetActive(true);

        movingFreezeClock.localPosition = _initialMovingFreezeClockPosition;

        snowFx.Play();

        Tween.LocalPosition(movingFreezeClock, timeText.rectTransform.localPosition, duration: 0.6f)
            .OnComplete(() =>
            {
                breakClockFx.gameObject.SetActive(true);
                breakClockFx.transform.localPosition = movingFreezeClock.transform.localPosition;
                breakClockFx.Play();

                movingFreezeClock.gameObject.SetActive(false);

                // progress bar
                _tweens.Add(Tween.Custom(0, 1, duration: 0.3f, onValueChange: newVal =>
                {
                    freezeProgressGroup.alpha = newVal;

                    _tweens.Add(Tween.Custom(1, 0, duration: 9.7f, ease: Ease.Linear, onValueChange: newVal =>
                    {
                        freezeProgressFill.fillAmount = newVal;
                        // freezeProgressFill.color = ColorUtil.WithAlpha(freezeProgressFill.color, 2 * (newVal - 0.08f));
                    }).OnComplete(() =>
                    {
                        _tweens.Add(Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
                        {
                            freezeProgressGroup.alpha = newVal;
                        })
                        .OnComplete(() =>
                        {
                            freezeProgressGroup.gameObject.SetActive(false);
                        })
                        );
                    }));
                }));

                _tweens.Add(Tween.Delay(10).OnComplete(() =>
                {
                    FreezedEffect(isFreeze: false);

                    unfreezeTimeEvent?.Invoke();

                    _isFreeze = false;
                }));

                FreezedEffect(isFreeze: true);
            });
    }

    private void FreezedEffect(bool isFreeze)
    {
        if (isFreeze)
        {
            freezedBackground.gameObject.SetActive(true);

            freezedBackground.color = ColorUtil.WithAlpha(freezedBackground.color, 0);

            _tweens.Add(Tween.Alpha(freezedBackground, 0.4f, duration: 0.3f));
        }
        else
        {
            _tweens.Add(Tween.Alpha(freezedBackground, 0, duration: 0.3f)
                .OnComplete(() =>
                {
                    freezedBackground.gameObject.SetActive(false);
                })
            );

            snowFx.Stop();
        }
    }

    private void Unfreeze()
    {
        CommonUtil.StopAllTweens(_tweens);

        FreezedEffect(isFreeze: false);

        unfreezeTimeEvent?.Invoke();

        _isFreeze = false;
    }

    private void Pause(bool isPaused)
    {
        _isPaused = isPaused;
    }

    private void Revive(BoosterType boosterType)
    {
        totalSecond = GamePersistentVariable.reviveAdditionalTime;

        _countingCoroutine = StartCoroutine(Counting());
    }

    string ConvertSecondsToMinutesSeconds(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60);

        return string.Format("{0:D2}:{1:D2}", minutes, remainingSeconds);
    }

    private void DebugLose()
    {
        if (_countingCoroutine != null)
        {
            StopCoroutine(_countingCoroutine);
        }

        loseLevelEvent?.Invoke();
    }
}
