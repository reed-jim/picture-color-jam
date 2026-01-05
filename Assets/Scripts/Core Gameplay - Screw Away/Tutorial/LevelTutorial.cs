using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using Lean.Localization;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelTutorial : MonoBehaviour
{
    [SerializeField] private Transform tutorialHand;
    // [SerializeField] private SpriteRenderer ripple;
    [SerializeField] private SpriteRenderer tutorialHandRenderer;

    [SerializeField] private Transform[] movePoints;

    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private TMP_Text tutorialText;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private int _step;
    #endregion

    #region EVENT
    public static event Action<bool> enableSwipingEvent;
    public static event Action<bool> enablePinchingEvent;
    #endregion

    void Awake()
    {
        BaseBlock.blockCompletedEvent += OnBlockCompleted;

        _tweens = new List<Tween>();

        StartCoroutine(TutorialStepOne());
    }

    void OnDestroy()
    {
        BaseBlock.blockCompletedEvent -= OnBlockCompleted;

        CommonUtil.StopAllTweens(_tweens);
    }

    private IEnumerator TutorialStepOne()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.3f);

        int phase = 0;
        bool isInTransition = false;

        // ripple.color = ColorUtil.WithAlpha(ripple.color, 0);
        // ripple.gameObject.SetActive(false);

        TutorialManager.Instance.ChangeCanvasRenderMode(RenderMode.ScreenSpaceCamera);
        TutorialManager.Instance.ShowOverlay();

        while (_step == 0)
        {
            if (isInTransition)
            {
                yield return waitForSeconds;

                continue;
            }

            if (phase == 0)
            {
                Tween.Position(tutorialHand, movePoints[1].position, duration: 0.6f)
                .OnComplete(() =>
                {
                    phase = 1;
                    isInTransition = false;
                });
            }
            else if (phase == 1)
            {
                Tween.Alpha(tutorialHandRenderer, 0, duration: 0.3f)
                .OnComplete(() =>
                {
                    tutorialHand.transform.position = movePoints[0].position;

                    Tween.Alpha(tutorialHandRenderer, 1, duration: 0.3f)
                    .OnComplete(() =>
                    {
                        phase = 0;
                        isInTransition = false;
                    });
                });
            }

            isInTransition = true;

            yield return waitForSeconds;
        }

        EndTutorial();
    }

    private void EndTutorial()
    {
        _tweens.Add(Tween.Alpha(tutorialCanvasGroup, 0, duration: 0.3f));
        _tweens.Add(Tween.Alpha(tutorialHandRenderer, 0, duration: 0.3f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        }));
    }

    #region CALLBACK
    private void OnBlockCompleted()
    {
        if (_step == 0)
        {
            TutorialManager.Instance.ChangeCanvasRenderMode(RenderMode.ScreenSpaceOverlay);
            TutorialManager.Instance.HideOverlay();

            _step++;
        }
    }
    #endregion
}
