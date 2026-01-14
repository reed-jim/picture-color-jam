using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : Singleton<CharacterUI>
{
    public Image expProgressBar;
    public TMP_Text expText;

    public CharacterData characterData;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        CharacterData defaultCharacterData = new CharacterData();

        defaultCharacterData.level = 1;
        defaultCharacterData.requiredEXP = 10 + defaultCharacterData.level;

        characterData = DataUtility.Load("characterData", defaultCharacterData);

        UpdateEXP(0);
        // StartCoroutine(GrantFreeLevel());
    }

    private IEnumerator GrantFreeLevel()
    {
        yield return new WaitUntil(() => GamePersistentVariable.isLoadingComplete);

        UpdateEXP(0);

        TowerManager.Instance.ShowTowerSelectionPopup();
    }

    public void UpdateEXP(float expAdded)
    {
        characterData.currentEXP += expAdded;

        float ratio = characterData.currentEXP / characterData.requiredEXP;

        Tween.Custom(expProgressBar.fillAmount, ratio, duration: 0.3f, onValueChange: newVal =>
        {
            expProgressBar.fillAmount = newVal;
            expText.text = $"{(int)(newVal * 100)}%";

            if (newVal >= 1)
            {
                characterData.level++;
                characterData.currentEXP = 0;
                characterData.requiredEXP = 10 + characterData.level;

                JoystickController.Instance.Hide();
                TowerManager.Instance.ShowTowerSelectionPopup();
            }
        });
    }
}

public class CharacterData
{
    public int level;
    public float currentEXP;
    public float requiredEXP;
}
