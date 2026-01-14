using System.Collections;
using UnityEngine;

public class ExplosiveProjectile : BaseProjectile
{
    public float launchForce = 10f;
    public float gravityScale = 1f;
    [SerializeField] private LayerMask hitLayerMask;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    public override void ShootAt(Vector2 targetPosition, float attackDamage)
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

        this.attackDamage = attackDamage;

        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        yield return new WaitForSeconds(0.8f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1, hitLayerMask);

        foreach (var hit in hits)
        {
            BaseEnemy enemy = hit.GetComponent<BaseEnemy>();

            if (enemy != null)
            {
                enemy.Hit(this);
            }
        }

        ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.blockMergeFx);

        fx.transform.position = transform.position;

        fx.Play();

        gameObject.SetActive(false);
    }

    Vector2 CalculateParabolaVelocity(Vector2 direction, float height)
    {
        float gravity = Physics2D.gravity.y * gravityScale;

        float timeUp = Mathf.Sqrt(-2 * height / gravity);
        float timeDown = Mathf.Sqrt(Mathf.Abs(2 * (direction.y - height) / gravity));
        float totalTime = timeUp + timeDown;

        Vector2 velocity;
        velocity.y = Mathf.Sqrt(-2 * gravity * height);
        velocity.x = direction.x / totalTime;

        return velocity;
    }
}
