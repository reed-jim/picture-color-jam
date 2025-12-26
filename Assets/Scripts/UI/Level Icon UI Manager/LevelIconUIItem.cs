using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelIconUIItem : MonoBehaviour
{
    public RectTransform container;
    public Image icon;
    public Image movingIcon;

    [SerializeField] private GameFactionSpriteContainer gameFactionSpriteContainer;

    private string _faction;

    public string Faction
    {
        get => _faction;
        set => _faction = value;
    }

    public void SetFaction(string faction)
    {
        icon.sprite = gameFactionSpriteContainer.factionSprites.First(item => item.name == faction);
        icon.SetNativeSize();

        bool isFitByX = icon.rectTransform.sizeDelta.x > icon.rectTransform.sizeDelta.y;

        Vector2 finalSize;

        if (isFitByX)
        {
            float ratioYToX = icon.rectTransform.sizeDelta.y / icon.rectTransform.sizeDelta.x;

            finalSize.x = (0.65f - 0.05f * ratioYToX) * container.sizeDelta.x;
            finalSize.y = ratioYToX * finalSize.x;
        }
        else
        {
            float ratioXToY = icon.rectTransform.sizeDelta.x / icon.rectTransform.sizeDelta.y;

            finalSize.y = (0.65f - 0.05f * ratioXToY) * container.sizeDelta.y;
            finalSize.x = ratioXToY * finalSize.y;
        }

        icon.rectTransform.sizeDelta = finalSize;

        movingIcon.sprite = icon.sprite;
        movingIcon.rectTransform.sizeDelta = icon.rectTransform.sizeDelta;

        movingIcon.gameObject.SetActive(false);

        _faction = faction;
    }

    public void Reset()
    {
        movingIcon.gameObject.SetActive(false);

        transform.localScale = Vector3.one;
    }
}
