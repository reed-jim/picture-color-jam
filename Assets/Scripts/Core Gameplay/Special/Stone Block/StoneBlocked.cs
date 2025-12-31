using PrimeTween;
using UnityEngine;

public class StoneBlocked : BaseBlock
{
    private bool _isInTransition;

    protected override void MoreLogicInAwake()
    {
        base.MoreLogicInAwake();

        BlockRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public override bool IsValidToMerge()
    {
        return false;
    }

    public override void Move(Vector2 direction)
    {
        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        _tweens.Add(Tween.Scale(transform, 1.03f * transform.localScale, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.15f)
        .OnComplete(() =>
        {
            _isInTransition = false;
        }));
    }

    public override void Stop()
    {
        
    }
}
