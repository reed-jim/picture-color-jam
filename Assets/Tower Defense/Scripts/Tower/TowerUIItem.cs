using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameEnum;

public class TowerUIItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform container;
    [SerializeField] private Button selectButton;
    public Image icon;

    public UpgradeContainer upgradeContainer;

    public TowerType towerType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!GamePersistentVariable.isPlacingTower)
        {
            SelectTower();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    // void Awake()
    // {
    //     selectButton.onClick.AddListener(SelectTower);
    // }

    private void SelectTower()
    {
        TowerManager.Instance.PlaceTower(this);
    }

    public void UpdateItem()
    {
        icon.sprite = upgradeContainer.towerTypeAndIconPairs.First(item => item.towerType == towerType).icon;
        UIUtil.SetSizeKeepRatioY(icon, 0.6f * container.sizeDelta.x);
        // icon.SetNativeSize();
    }
}
