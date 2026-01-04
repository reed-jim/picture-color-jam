using UnityEngine;

public class BaseProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D projectileRigidbody;

    public float force;

    public void ShootAt(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - projectileRigidbody.position).normalized;
        projectileRigidbody.AddForce(direction * force, ForceMode2D.Impulse);
    }
}
