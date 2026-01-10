using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameEnum;

public class TowerUIItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button selectButton;

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
        TowerManager.Instance.PlaceTower();
    }
}
