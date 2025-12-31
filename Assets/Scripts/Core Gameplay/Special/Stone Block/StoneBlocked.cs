using PrimeTween;
using UnityEngine;

public class StoneBlocked : BaseBlock
{
    protected override void MoreLogicInAwake()
    {
        base.MoreLogicInAwake();

        BlockRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public override bool IsValidToMerge()
    {
        return false;
    }

    public override bool IsMovable()
    {
        return false;
    }

    public override void Stop()
    {

    }
}
