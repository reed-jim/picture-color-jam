using UnityEngine;

public class ExplosiveProjectile : BaseProjectile
{
    public float launchForce = 10f;
    public float gravityScale = 1f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    public override void ShootAt(Vector2 targetPosition)
    {
        try
        {
            Vector2 direction = targetPosition - (Vector2)transform.position;

            float height = 2f;

            Vector2 velocity = CalculateParabolaVelocity(direction, height);
            rb.linearVelocity = velocity;
        }
        catch (System.Exception)
        {

        }
    }

    Vector2 CalculateParabolaVelocity(Vector2 direction, float height)
    {
        float gravity = Physics2D.gravity.y * gravityScale;

        float timeUp = Mathf.Sqrt(-2 * height / gravity);
        float timeDown = Mathf.Sqrt(2 * (direction.y - height) / gravity);
        float totalTime = timeUp + timeDown;

        Vector2 velocity;
        velocity.y = Mathf.Sqrt(-2 * gravity * height);
        velocity.x = direction.x / totalTime;

        return velocity;
    }
}
