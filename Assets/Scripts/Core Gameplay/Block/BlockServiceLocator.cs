using UnityEngine;
using UnityEngine.Rendering;

public class BlockServiceLocator : MonoBehaviour
{
    public BaseBlock block;
    public BlockFaction blockFaction;
    public BlockCollider blockCollider;
    public BlockMaterialPropertyBlock blockMaterialPropertyBlock;

    public SpriteRenderer spriteRenderer;
    public Collider2D blockCollider2D;

    #region Layer
    [SerializeField] public SpriteRenderer iconRenderer;
    [SerializeField] public SpriteRenderer outlineRenderer;
    [SerializeField] public SortingGroup iconMaskSortingGroup;
    #endregion

    public Vector3 Size
    {
        get => spriteRenderer.bounds.size;
    }

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        block = GetComponent<BaseBlock>();
        blockFaction = GetComponent<BlockFaction>();
        blockCollider = GetComponent<BlockCollider>();
        blockMaterialPropertyBlock = GetComponent<BlockMaterialPropertyBlock>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
