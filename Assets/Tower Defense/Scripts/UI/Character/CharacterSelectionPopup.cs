using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionPopup : BasePopup
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private TMP_Text nameText;

    public MainCharacterDataContainer mainCharacterDataContainer;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        selectButton.onClick.AddListener(Select);
        nextButton.onClick.AddListener(Next);
        prevButton.onClick.AddListener(Prev);
    }

    private void Select()
    {
        CharacterManager.Instance.SelectMainCharacter();

        Hide();
    }

    private void Next()
    {
        bool isValid = CharacterManager.Instance.Next();

        if (isValid)
        {
            UpdateNewCharacter();
        }
        else
        {
            ShowNoMoreCharacterLeft();
        }
    }

    private void Prev()
    {
        bool isValid = CharacterManager.Instance.Prev();

        if (isValid)
        {
            UpdateNewCharacter();
        }
        else
        {
            ShowNoMoreCharacterLeft();
        }
    }

    private void UpdateNewCharacter()
    {
        int index = CharacterManager.Instance.selectedMainCharacterIndex;

        nameText.text = $"{mainCharacterDataContainer.items[index].characterName}";
    }

    private void ShowNoMoreCharacterLeft()
    {
        ShowNotification(GameConstants.WARNING, GameConstants.noMoreCharacterLeft);
    }
}
