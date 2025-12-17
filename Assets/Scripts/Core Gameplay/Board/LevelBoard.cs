using UnityEngine;

public class LevelBoard : MonoBehaviour
{
    public SpriteRenderer ground;
    public BoxCollider2D leftEdgeCollider;
    public BoxCollider2D rightEdgeCollider;
    public BoxCollider2D topEdgeCollider;
    public BoxCollider2D bottomEdgeCollider;

    public int numColumn;
    public int numRow;
}
