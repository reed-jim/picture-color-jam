using PrimeTween;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class SpecialLevelTutorial : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private SkeletonGraphic glow;
    [SerializeField] private Button continueButton;

    void Awake()
    {
        continueButton.onClick.AddListener(Continue);
    }

    private void Continue()
    {
        Tween.Alpha(glow, 0, duration: 0.3f);
        Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
        {
            canvasGroup.alpha = newVal;
        })
        .OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
