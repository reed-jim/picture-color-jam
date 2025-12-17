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

    void Awake()
    {
        GamePersistentVariable.levelBoard = this;

        // center the level
        Transform level = transform.parent;

        level.transform.position -= new Vector3(transform.position.x, 0, 0);
    }
}
