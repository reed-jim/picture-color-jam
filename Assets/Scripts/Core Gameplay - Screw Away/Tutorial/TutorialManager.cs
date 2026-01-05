using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : Singleton<TutorialManager>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image overlay;

    public void ChangeCanvasRenderMode(RenderMode renderMode)
    {
        canvas.renderMode = renderMode;
    }

    public void ShowOverlay()
    {
        overlay.gameObject.SetActive(true);
        overlay.color = ColorUtil.WithAlpha(overlay.color, 0);

        Tween.Alpha(overlay, 0.95f, duration: 0.3f);
    }

    public void HideOverlay()
    {
        Tween.Alpha(overlay, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            overlay.gameObject.SetActive(false);
        });
    }
}
