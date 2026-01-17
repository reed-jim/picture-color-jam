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
            Tween.PositionY(text.transform, text.transform.position.y + 0.5f, duration: 0.3f)
            .OnComplete(() =>
            {
                text.alpha = 1;

                ObjectPoolingEverything.ReturnToPool(GameConstants.damageText, gameObject);

                // TweenUtil.PunchHide(text.transform, 1, onCompleted: () =>
                // {
                //     isInTransition = false;
                // });
            });

            Tween.Alpha(text, 0, duration: 0.3f);
        });
    }
}
