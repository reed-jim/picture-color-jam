using UnityEngine;

[CreateAssetMenu(fileName = "BlockPreviewSpriteContainer", menuName = "Scriptable Objects/BlockPreviewSpriteContainer")]
public class BlockPreviewSpriteContainer : ScriptableObject
{
    [SerializeField] private Sprite[] blockPreviewSnapSprites;

    public Sprite[] BlockPreviewSnapSprites
    {
        get => blockPreviewSnapSprites;
    }
}
