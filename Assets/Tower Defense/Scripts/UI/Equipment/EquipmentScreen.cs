using System.Collections.Generic;
using UnityEngine;

public class EquipmentScreen : MonoBehaviour
{
    public CanvasGroup characterArea;
    public CanvasGroup gearArea;

    public List<EquipmentUIItem> equipmentUIItems;
    public EquipmentDataContainer equipmentDataContainer;

    void Awake()
    {
        equipmentUIItems = new List<EquipmentUIItem>();
    }

    public void UpdateGears()
    {
        for (int i = 0; i < equipmentDataContainer.items.Length; i++)
        {
            int index = i;

            EquipmentData equipmentData = equipmentDataContainer.items[i];

            EquipmentUIItem equipmentUIItem;

            if (IsListContainIndex(equipmentUIItems, index))
            {
                equipmentUIItem = equipmentUIItems[index];
            }
            else
            {
                equipmentUIItem = ObjectPoolingEverything.GetFromPool<EquipmentUIItem>(GameConstants.equipmentUIItem);

                equipmentUIItems.Add(equipmentUIItem);
            }

            if (equipmentData.isOwned)
            {
                equipmentUIItem.Unlock();
                equipmentUIItem.SetData(equipmentData);
            }
            else
            {
                equipmentUIItem.Lock();
            }
        }
    }

    private bool IsListContainIndex<T>(List<T> list, int index)
    {
        return index < list.Count;
    }
}
