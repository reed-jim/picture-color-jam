using PrimeTween;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public TMP_Text text;

    public bool isInTransition;

    public void ShowText(float damage, Vector3 position)
    {
        if (isInTransition)
        {
            return;
        }

        DamageTextContainer.Instance.AddDamageText(this);

        text.text = $"{damage}";
        text.transform.position = position;

        isInTransition = true;

        TweenUtil.PunchShow(text.transform, Vector3.one, onCompleted: () =>
        {
            Tween.Delay(0.6f).OnComplete(() =>
            {
                TweenUtil.PunchHide(text.transform, 1, onCompleted: () =>
                {
                    isInTransition = false;
                });
            });
        });
    }
}
