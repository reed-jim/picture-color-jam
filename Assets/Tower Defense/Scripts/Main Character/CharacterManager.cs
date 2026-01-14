using System;
using UnityEngine;
using static GameEnum;

public class CharacterManager : Singleton<CharacterManager>
{
    public MainCharacter mainCharacter;

    public MainCharacter[] characterPool;

    public int selectedMainCharacterIndex;

    public static Action<ScreenRoute> switchRouteEvent;

    void Start()
    {
        switchRouteEvent?.Invoke(ScreenRoute.CharacterSelection);
    }

    public void SelectMainCharacter()
    {
        if (mainCharacter != null)
        {
            Destroy(mainCharacter.gameObject);
        }

        mainCharacter = Instantiate(characterPool[selectedMainCharacterIndex]);
    }

    public bool Next()
    {
        if (selectedMainCharacterIndex + 1 < characterPool.Length)
        {
            selectedMainCharacterIndex++;

            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Prev()
    {
        if (selectedMainCharacterIndex - 1 >= 0)
        {
            selectedMainCharacterIndex--;

            return true;
        }
        else
        {
            return false;
        }
    }
}
