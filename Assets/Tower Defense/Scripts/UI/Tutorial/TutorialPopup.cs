using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : BasePopup
{
    public TMP_Text description;
    public Button okButton;

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        okButton.onClick.AddListener(Hide);
    }
}
