using PrimeTween;
using TMPro;
using UnityEngine;

public class FreezeBlock : BaseBlock
{
    [Header("FREEZE BLOCK")]
    [SerializeField] private int remainingBlockToMelt = 3;
    [SerializeField] private SpriteRenderer ice;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text remainingBlockToMeltText;
    // [SerializeField] private ParticleSystem breakIceFx;

    private bool _isFreeze;

    public int RemainingBlockToMelt
    {
        get => remainingBlockToMelt;
        set => remainingBlockToMelt = value;
    }

    protected override void MoreLogicInAwake()
    {
        BlockManager.mergeBlockEvent += OnBlockDisintegrated;

        _isFreeze = true;

        remainingBlockToMeltText.text = $"{remainingBlockToMelt}";
    }

    protected override void MoreLogicOnDestroy()
    {
        BlockManager.mergeBlockEvent -= OnBlockDisintegrated;
    }

    public override bool IsMovable()
    {
        return !_isFreeze;
    }

    // public override void Move(Vector2 direction)
    // {
    // if (_isFreeze)
    // {
    //     if (_isInTransition)
    //     {
    //         return;
    //     }
    //     else
    //     {
    //         _isInTransition = true;
    //     }

    //     _tweens.Add(Tween.Scale(transform, 1.03f * transform.localScale, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.3f)
    //     .OnComplete(() =>
    //     {
    //         _isInTransition = false;
    //     }));

    //     return;
    // }

    //     base.Move(direction);
    // }

    public override void Stop()
    {
        if (_isFreeze)
        {
            return;
        }

        base.Stop();
    }

    public override void SnapImmediately()
    {
        base.SnapImmediately();

        ice.sortingOrder = BlockServiceLocator.iconMaskSortingGroup.sortingOrder;
        canvas.sortingOrder = BlockServiceLocator.spriteRenderer.sortingOrder + 2;
    }

    public override bool IsValidToMerge()
    {
        return !_isFreeze;
    }

    private void OnBlockDisintegrated()
    {
        if (remainingBlockToMelt <= 0)
        {
            return;
        }

        remainingBlockToMelt--;

        if (remainingBlockToMelt == 0)
        {
            _isFreeze = false;

            TweenUtil.PunchHide(remainingBlockToMeltText.transform);
            Tween.Alpha(ice, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                canvas.gameObject.SetActive(false);
                ice.gameObject.SetActive(false);
            });
        }

        remainingBlockToMeltText.text = $"{remainingBlockToMelt}";
    }

    public void RefreshRemainingBlockToMeltText()
    {
        remainingBlockToMeltText.text = $"{remainingBlockToMelt}";
    }
}
