using UnityEngine;

public class BaseProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D projectileRigidbody;

    public float force;
    public float attackDamage;

    public virtual void ShootAt(Vector2 targetPosition, float attackDamage)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        projectileRigidbody.AddForce(direction * force, ForceMode2D.Impulse);

        this.attackDamage = attackDamage;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // gameObject.SetActive(false);
    }
}
