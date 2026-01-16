using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteScreenFitter : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        ScaleSpriteToScreen();
    }

    public void ScaleSpriteToScreen()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (_spriteRenderer.sprite == null)
        {
            return;
        }

        // Maintain aspect ratio 
        Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;

        Vector2 screenSize = GetScreenSizePerspective(Camera.main);

        float scaleX = screenSize.x / spriteSize.x;
        float scaleY = screenSize.y / spriteSize.y;

        float finalScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    Vector2 GetScreenSizePerspective(Camera cam)
    {
        float distance = Mathf.Abs(cam.transform.position.z);

        float height = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * cam.aspect;

        return new Vector2(width, height);
    }
}
