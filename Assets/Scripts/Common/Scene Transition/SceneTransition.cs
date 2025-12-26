using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform loadingContent;
    [SerializeField] private RectTransform progressFillRT;
    [SerializeField] private RectTransform loadingTextRT;
    [SerializeField] private Image fadeBackground;
    [SerializeField] private CanvasGroup blackBackground;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image progressBarFill;
    // [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private SkeletonGraphic loadingSpine;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("LOADING LEVEL IN PROGRESS")]
    [SerializeField] private RectTransform loadLevelInProgressContainer;
    [SerializeField] private CanvasGroup loadLevelInProgressCanvasGroup;
    [SerializeField] private RectTransform rotatingLoadingIcon;
    [SerializeField] private Image rotatingLoadingIconImage;
    [SerializeField] private Image cloneRotatingLoadingIconImage;
    private Coroutine _loadingLevelInProgressCoroutine;
    private Vector3 _rotatingLoadingIconInitialPosition;
    [SerializeField] private Sprite[] blockSpritePool;

    [Header("FAKE LOADING FOR ADS")]
    [SerializeField] private Image blackBackgroundToPassPolicy;

    [Header("CUSTOMIZE")]
    [SerializeField] private float startDelay;
    [SerializeField] private float blackScreenStartDelay;
    [SerializeField] private float fadeOutDuration;

    private Vector2 _canvasSize;
    private List<Tween> _tweens;
    private bool _isFirstTimeOpenApp;

    [SerializeField] private IntVariable currentLevel;


    private void Awake()
    {
        LevelLoader.showSceneTransitionEvent += BlackScreenTransition;

        _tweens = new List<Tween>();

        _canvasSize = canvas.sizeDelta;

        progressBar.value = 0;
        progressBarFill.color = ColorUtil.WithAlpha(progressBarFill.color, 0);

        _isFirstTimeOpenApp = DataUtility.Load(GameConstants.IS_FIRST_TIME_OPEN_APP, true);

        // if (!_isFirstTimeOpenApp)
        // {
        //     BlackScreenTransition();
        // }
        // else
        // {
        //     blackBackground.gameObject.SetActive(false);

        //     DataUtility.Save(GameConstants.IS_FIRST_TIME_OPEN_APP, false);
        // }

        if (_isFirstTimeOpenApp)
        {
            DataUtility.Save(GameConstants.IS_FIRST_TIME_OPEN_APP, false);
        }

        loadLevelInProgressContainer.gameObject.SetActive(false);
        blackBackgroundToPassPolicy.gameObject.SetActive(false);
    }

    // void Start()
    // {
    //     _rotatingLoadingIconInitialPosition = rotatingLoadingIcon.localPosition;
    // }

    private void OnDestroy()
    {
        LevelLoader.showSceneTransitionEvent -= BlackScreenTransition;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void Transition()
    {
        container.gameObject.SetActive(true);

        // _tweens.Add(Tween.Alpha(progressBarFill, 0, 1, duration: 0.2f * startDelay));

        _tweens.Add(Tween.Custom(0, 1, duration: startDelay, ease: Ease.Linear, onValueChange: newVal =>
        {
            progressBar.value = newVal;
            progressBarFill.color = ColorUtil.WithAlpha(progressBarFill.color, newVal * 2);

            loadingText.text = $"{(int)(newVal * 100)}%";
        }));

        _tweens.Add(Tween.Delay(startDelay).OnComplete(
            () =>
            {
                _tweens.Add(
                    Tween.Custom(1, 0, duration: fadeOutDuration, onValueChange: newVal =>
                    {
                        canvasGroup.alpha = newVal;
                        loadingSpine.color = ColorUtil.WithAlpha(loadingSpine.color, newVal);
                    })
                    .OnComplete(() =>
                    {
                        container.gameObject.SetActive(false);
                    })
                );
            }
        ));

        //     SaferioTween.DelayAsync(startDelay, onCompletedAction: (() =>
        //     {
        //         _tweens.Add(
        //             Tween.Custom(1, 0, duration: fadeOutDuration, onValueChange: newVal =>
        //             {
        //                 canvasGroup.alpha = newVal;
        //                 loadingSpine.color = ColorUtil.WithAlpha(loadingSpine.color, newVal);
        //             })
        //             .OnComplete(() =>
        //             {
        //                 container.gameObject.SetActive(false);
        //             })
        //         );

        //         // SaferioTween.LocalPositionAsync(loadingContent, new Vector3(0, -_canvasSize.y), duration: 0.6f * fadeOutDuration);
        //         // SaferioTween.AlphaAsync(fadeBackground, 0, duration: fadeOutDuration, onCompletedAction: () =>
        //         // {
        //         //     container.gameObject.SetActive(false);
        //         // });
        //     }));
    }

    private void BlackScreenTransition()
    {
        BlackScreenTransition(blackScreenStartDelay, onCompletedAction: null);

        // if (GamePersistentVariable.remoteConfigIsShowMREC && !UserData.IsRemoveAds && !_isFirstTimeOpenApp && AdmobAdsMax.Instance.IsValidShowMREC())
        // {
        //     LoadingLevelInProgress();
        // }
        // else
        // {
        //     loadLevelInProgressContainer.gameObject.SetActive(false);

        //     _isFirstTimeOpenApp = false;
        // }

        blackBackgroundToPassPolicy.gameObject.SetActive(false);

        if (_isFirstTimeOpenApp)
        {
            _isFirstTimeOpenApp = false;

            return;
        }

        LoadingLevelInProgress();

        // if (GamePersistentVariable.doubleInterAdsConfig.isEnable && currentLevel.Value >= GamePersistentVariable.doubleInterAdsConfig.minLevel)
        // {
        //     LoadingLevelInProgress();
        // }
        // else
        // {
        //     loadLevelInProgressContainer.gameObject.SetActive(false);
        // }
    }

    private void BlackScreenTransition(float delay, Action onCompletedAction)
    {
        blackBackground.gameObject.SetActive(true);

        blackBackground.alpha = 1;

        Tween.Alpha(blackBackground, 1, 0, startDelay: delay, duration: fadeOutDuration).OnComplete(() =>
        {
            blackBackground.gameObject.SetActive(false);

            onCompletedAction?.Invoke();
        });
    }

    #region LOADING LEVEL IN PROGRESS
    private void LoadingLevelInProgress()
    {
        loadLevelInProgressContainer.gameObject.SetActive(true);

        loadLevelInProgressCanvasGroup.alpha = 1;

        if (_loadingLevelInProgressCoroutine != null)
        {
            StopCoroutine(_loadingLevelInProgressCoroutine);
        }

        _loadingLevelInProgressCoroutine = StartCoroutine(RotatingIcon());

        AdmobAdsMax.Instance.ShowMRec();

        float minWaitTime = 3;

        if (GamePersistentVariable.remoteConfigIsShowMREC)
        {
            minWaitTime = 4;
        }

        _tweens.Add(Tween.Delay(minWaitTime).OnComplete(() =>
        {
            AdmobAdsMax.Instance.HideMRec();

            // double inter
            blackBackgroundToPassPolicy.gameObject.SetActive(true);

            AdmobAdsMax.Instance.ShowDoubleInterstitialAds(
                onDoubleInterClosed: () =>
                {
                    blackBackgroundToPassPolicy.gameObject.SetActive(false);
                },
                isShowBreakAds: true
            );

            _tweens.Add(Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
            {
                loadLevelInProgressCanvasGroup.alpha = newVal;
            })
            .OnComplete(() =>
                {
                    loadLevelInProgressContainer.gameObject.SetActive(false);
                })
            );

            StopCoroutine(_loadingLevelInProgressCoroutine);
        }));
    }

    private IEnumerator RotatingIcon()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1 / 60f);

        int step = 0;

        bool isSwitchingColor = false;

        // rotatingLoadingIconImage.sprite = blockSpritePool[UnityEngine.Random.Range(0, blockSpritePool.Length)];

        rotatingLoadingIcon.eulerAngles = Vector3.zero;

        while (true)
        {
            rotatingLoadingIcon.eulerAngles -= new Vector3(0, 0, 8);

            // if (!isSwitchingColor)
            // {
            //     cloneRotatingLoadingIconImage.gameObject.SetActive(true);

            //     cloneRotatingLoadingIconImage.sprite = rotatingLoadingIconImage.sprite;

            //     cloneRotatingLoadingIconImage.color = Color.white;

            //     Tween.Alpha(cloneRotatingLoadingIconImage, 0, duration: 0.75f)
            //     .OnComplete(() =>
            //     {
            //         cloneRotatingLoadingIconImage.gameObject.SetActive(false);
            //     });

            //     rotatingLoadingIconImage.sprite = blockSpritePool[UnityEngine.Random.Range(0, blockSpritePool.Length)];

            //     rotatingLoadingIconImage.color = ColorUtil.WithAlpha(Color.white, 0);

            //     _tweens.Add(
            //         Tween.Alpha(rotatingLoadingIconImage, 1, duration: 0.75f)
            //         .OnComplete(() => isSwitchingColor = false)
            //         );

            //     isSwitchingColor = true;
            // }

            // step++;

            // if (step > 3)
            // {
            //     step = 0;
            // }

            yield return waitForSeconds;
        }
    }
    #endregion
}
