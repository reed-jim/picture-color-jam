using UnityEngine;

public class BaseProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D projectileRigidbody;

    public float force;
    public float attackDamage;

    public virtual void ShootAt(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        projectileRigidbody.AddForce(direction * force, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // gameObject.SetActive(false);
    }
}
