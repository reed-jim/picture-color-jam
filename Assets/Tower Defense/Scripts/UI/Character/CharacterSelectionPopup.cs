using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using static GameEnum;

public class CharacterSelectionPopup : BasePopup
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private TMP_Text nameText;
    public VideoPlayer videoPlayer;
    public Image staticIcon;

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

        Hide(onCompletedAction: () =>
        {
            SwitchRoute(ScreenRoute.Tutorial);
        });
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

        MainCharacterData mainCharacterData = mainCharacterDataContainer.items[index];

        nameText.text = $"{mainCharacterData.characterName}";

        staticIcon.gameObject.SetActive(true);
        staticIcon.sprite = mainCharacterData.icon;
        staticIcon.color = Color.white;

        videoPlayer.url = mainCharacterData.videoUrl;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepareCompleted;

        void OnVideoPrepareCompleted(VideoPlayer videoPlayer)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepareCompleted;

            Tween.Alpha(staticIcon, 0, duration: 0.3f, startDelay: 0.3f).OnComplete(() =>
            {
                staticIcon.gameObject.SetActive(false);
            });
        }
    }

    private void ShowNoMoreCharacterLeft()
    {
        ShowNotification(GameConstants.WARNING, GameConstants.noMoreCharacterLeft);
    }
}
