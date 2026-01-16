using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentScreen : Singleton<EquipmentScreen>
{
    public CanvasGroup characterArea;
    public CanvasGroup gearArea;

    public List<EquipmentUIItem> equipmentUIItems;
    public EquipmentUIItem[] characterEquipmentUIItems;

    public CharacterEquipmentData MainCharacterEquipmentData;

    public EquipmentDataContainer equipmentDataContainer;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        equipmentUIItems = new List<EquipmentUIItem>();

        CharacterEquipmentData defaultCharacterEquipmentData = new CharacterEquipmentData();

        defaultCharacterEquipmentData.characterEquipmentSlots = new CharacterEquipmentSlot[4];

        defaultCharacterEquipmentData.characterEquipmentSlots[0] = new CharacterEquipmentSlot()
        {
            equipmentType = EquipmentType.Weapon,
            equipmentId = "swing 1"
        };

        defaultCharacterEquipmentData.characterEquipmentSlots[1] = new CharacterEquipmentSlot()
        {
            equipmentType = EquipmentType.Helmet,
            equipmentId = "swing 1"
        };

        defaultCharacterEquipmentData.characterEquipmentSlots[2] = new CharacterEquipmentSlot()
        {
            equipmentType = EquipmentType.Accessory,
            equipmentId = "swing 1"
        };

        defaultCharacterEquipmentData.characterEquipmentSlots[3] = new CharacterEquipmentSlot()
        {
            equipmentType = EquipmentType.Offhand,
            equipmentId = "swing 1"
        };

        MainCharacterEquipmentData = DataUtility.Load("MainCharacterEquipmentData", defaultCharacterEquipmentData);

        for (int i = 0; i < characterEquipmentUIItems.Length; i++)
        {
            characterEquipmentUIItems[i].Unlock();
            characterEquipmentUIItems[i].SetData(GetEquipmentData(MainCharacterEquipmentData.characterEquipmentSlots[i].equipmentId));
        }
    }

    void Start()
    {
        UpdateGears();
    }

    public void UpdateGears()
    {
        if (equipmentUIItems.Count < equipmentDataContainer.items.Length)
        {
            for (int i = 0; i < equipmentDataContainer.items.Length; i++)
            {
                EquipmentUIItem equipmentUIItem = ObjectPoolingEverything.GetFromPool<EquipmentUIItem>(GameConstants.equipmentUIItem);

                equipmentUIItem.transform.SetParent(gearArea.transform);
                equipmentUIItem.transform.localScale = Vector3.one;

                equipmentUIItems.Add(equipmentUIItem);
            }
        }

        for (int i = 0; i < equipmentDataContainer.items.Length; i++)
        {
            int index = i;

            EquipmentData equipmentData = equipmentDataContainer.items[index];

            EquipmentUIItem equipmentUIItem = equipmentUIItems[index];

            equipmentUIItem.SetData(equipmentData);

            if (equipmentData.isOwned)
            {
                equipmentUIItem.Unlock();
            }
            else
            {
                equipmentUIItem.Lock();
            }
        }
    }

    public void Equip(EquipmentData equipmentData)
    {
        for (int i = 0; i < MainCharacterEquipmentData.characterEquipmentSlots.Length; i++)
        {
            CharacterEquipmentSlot characterEquipmentSlot = MainCharacterEquipmentData.characterEquipmentSlots[i];

            if (characterEquipmentSlot.equipmentType == equipmentData.equipmentType)
            {
                characterEquipmentSlot.equipmentId = equipmentData.id;

                characterEquipmentUIItems[i].SetData(equipmentData);

                break;
            }
        }
    }

    public EquipmentData GetEquipmentData(string id)
    {
        return equipmentDataContainer.items.First(item => item.id == id);
    }

    private bool IsListContainIndex<T>(List<T> list, int index)
    {
        return index < list.Count;
    }
}
