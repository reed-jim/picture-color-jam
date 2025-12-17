using System;
using PrimeTween;
using UnityEngine;

public static class TweenUtil
{
    public static Tween PunchShow(Transform target, Vector3 targetScale, Action onCompleted = null)
    {
        target.gameObject.SetActive(true);
        target.transform.localScale = Vector3.zero;

        return Tween.Scale(target, 1.1f * targetScale, duration: 0.2f, ease: Ease.InOutSine)
        .OnComplete(() =>
        {
            Tween.Scale(target, targetScale, duration: 0.2f, ease: Ease.InOutSine).OnComplete(() =>
            {
                onCompleted?.Invoke();
            });
        });
    }

    public static Tween PunchHide(Transform target, Action onCompleted = null)
    {
        Vector3 currentScale = target.localScale;

        return Tween.Scale(target, 1.1f * currentScale, duration: 0.2f, ease: Ease.InOutSine)
        .OnComplete(() =>
        {
            Tween.Scale(target, 0, duration: 0.2f, ease: Ease.InOutSine).OnComplete(() =>
            {
                target.gameObject.SetActive(false);

                onCompleted?.Invoke();
            });
        });
    }
}
