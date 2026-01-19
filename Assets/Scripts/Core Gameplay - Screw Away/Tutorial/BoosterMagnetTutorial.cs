using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class BoosterMagnetTutorial : BoosterTutorial
{
    [SerializeField] private Transform tutorialHandWorldSpace;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    private bool _isUsedMagnet;

    protected override void RegisterMoreEvent()
    {
        BlockSelectionInput.startVacumnEvent += OnMagnetBoosterUsed;
    }

    protected override void UnregisterMoreEvent()
    {
        BlockSelectionInput.startVacumnEvent -= OnMagnetBoosterUsed;
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
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);

        int phase = 0;

        while (!_isUsedMagnet)
        {
            if (phase == 0)
            {
                Tween.Scale(tutorialHandWorldSpace, 1.3f, duration: 0.3f)
                .OnComplete(() =>
                {
                    phase = 1;
                });
            }
            else if (phase == 1)
            {
                Tween.Scale(tutorialHandWorldSpace, 1f, duration: 0.3f)
                .OnComplete(() =>
                {
                    phase = 0;
                });
            }

            yield return waitForSeconds;
        }
    }

    private void OnMagnetBoosterUsed()
    {
        _isUsedMagnet = true;

        TweenUtil.PunchHide(tutorialHandWorldSpace);
    }
}
