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

        movingIcon.sprite = icon.sprite;
        movingIcon.SetNativeSize();

        movingIcon.gameObject.SetActive(false);

        _faction = faction;
    }

    public void Reset()
    {
        movingIcon.gameObject.SetActive(false);

        transform.localScale = Vector3.one;
    }
}
