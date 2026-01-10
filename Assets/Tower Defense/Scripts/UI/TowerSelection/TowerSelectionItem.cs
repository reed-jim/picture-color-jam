using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class TowerSelectionItem : MonoBehaviour
{
    public Button selectButton;
    public Image icon;
    public TMP_Text description;

    public TowerType towerType;
    public bool isBuff;
    public float buffRatio;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void Select()
    {
        TowerManager.Instance.AddTowerToPool(towerType);
    }
}
