using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class BoosterTeleportTutorial : BoosterTutorial
{
    [SerializeField] private Transform tutorialHandWorldSpace;
    [SerializeField] private Transform exampleBlock;
    [SerializeField] private SpriteRenderer tutorialHandRenderer;
    [SerializeField] private SpriteRenderer exampleBlockRenderer;
    [SerializeField] private SpriteRenderer exampleBlockIconRenderer;
    [SerializeField] private Transform[] handMovePoints;
    [SerializeField] private Transform[] exampleBlockMovePoints;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    private bool _isUsedTeleport;

    protected override void RegisterMoreEvent()
    {
        BaseBlock.useTeleportBoosterEvent += OnTeleportBoosterUsed;
        BoosterUI.disableBreakObjectModeEvent += OnTeleportBoosterUsed;
    }

    protected override void UnregisterMoreEvent()
    {
        BaseBlock.useTeleportBoosterEvent -= OnTeleportBoosterUsed;
        BoosterUI.disableBreakObjectModeEvent -= OnTeleportBoosterUsed;
    }

    protected override void OnBoosterClick()
    {
        graphicRaycaster.enabled = false;
        boosterButton.gameObject.SetActive(false);

        TweenUtil.PunchShow(tutorialHandWorldSpace, Vector3.one, onCompleted: () =>
        {
            StartCoroutine(PlayTutorialHandWorldSpaceAnimation());
        });
    }

    private IEnumerator PlayTutorialHandWorldSpaceAnimation()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.3f);

        int phase = 0;
        bool isInTransition = false;

        while (!_isUsedTeleport)
        {
            if (isInTransition)
            {
                yield return waitForSeconds;

                continue;
            }

            if (phase == 0)
            {
                exampleBlock.gameObject.SetActive(true);
                exampleBlock.position = exampleBlockMovePoints[0].transform.position;
                exampleBlockRenderer.color = ColorUtil.WhiteWithAlpha(100f / 255);
                exampleBlockIconRenderer.color = ColorUtil.WhiteWithAlpha(160f / 255);

                Tween.Position(tutorialHandWorldSpace, handMovePoints[1].position, duration: 0.6f)
                .OnComplete(() =>
                {
                    phase = 1;
                    isInTransition = false;
                });

                Tween.Position(exampleBlock, exampleBlockMovePoints[1].position, duration: 0.6f);
            }
            else if (phase == 1)
            {
                Tween.Alpha(tutorialHandRenderer, 0, duration: 0.3f)
                .OnComplete(() =>
                {
                    tutorialHandWorldSpace.transform.position = handMovePoints[0].position;

                    Tween.Alpha(tutorialHandRenderer, 1, duration: 0.3f)
                    .OnComplete(() =>
                    {
                        phase = 0;
                        isInTransition = false;
                    });
                });

                Tween.Alpha(exampleBlockRenderer, 0, duration: 0.3f);
                Tween.Alpha(exampleBlockIconRenderer, 0, duration: 0.3f);
            }

            isInTransition = true;

            yield return waitForSeconds;
        }
    }

    private void OnTeleportBoosterUsed()
    {
        _isUsedTeleport = true;

        TweenUtil.PunchHide(tutorialHandWorldSpace);

        exampleBlock.gameObject.SetActive(false);
    }
}
