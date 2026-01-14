using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class LazyHealthBar : MonoBehaviour
{
    public RectTransform container;
    public Image healthBar;
    public Image lazyHealthBar;

    public float prevValue;

    private Tween healthBarTween;
    private Tween lazyHealthBarTween;

    void Start()
    {
        prevValue = 1;

        container.gameObject.SetActive(false);
    }

    public void ChangeValue(float value)
    {
        if (!container.gameObject.activeInHierarchy)
        {
            container.gameObject.SetActive(true);
        }

        if (healthBarTween.isAlive)
        {
            healthBarTween.Stop();
        }

        if (lazyHealthBarTween.isAlive)
        {
            lazyHealthBarTween.Stop();
        }

        healthBarTween = Tween.Custom(healthBar.fillAmount, value, duration: 0.3f, onValueChange: newVal =>
        {
            healthBar.fillAmount = newVal;
        });

        lazyHealthBarTween = Tween.Custom(lazyHealthBar.fillAmount, value, duration: 1f, onValueChange: newVal =>
        {
            lazyHealthBar.fillAmount = newVal;
        });
    }
}
