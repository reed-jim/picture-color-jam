using PrimeTween;
using TMPro;
using UnityEngine;

public class WoodenBlock : BaseBlock
{
    [SerializeField] private Canvas canvas;

    public override bool IsValidToMerge()
    {
        return false;
    }

    public override void SnapImmediately()
    {
        base.SnapImmediately();

        canvas.sortingOrder = BlockServiceLocator.spriteRenderer.sortingOrder + 2;
    }
}
