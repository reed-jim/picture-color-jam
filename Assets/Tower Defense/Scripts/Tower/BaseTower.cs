using System.Collections;
using UnityEngine;

public enum ProjectileType
{
    Normal,
    Explosive
}

public class BaseTower : MonoBehaviour
{
    public bool isActive;
    public float attackRadius;
    public ProjectileType projectileType;
    public LayerMask targetLayer;

    void Start()
    {
        StartCoroutine(AutoShooting());
    }

    private IEnumerator AutoShooting()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        while (true)
        {
            if (isActive)
            {
                Transform target = FindNearestTarget();

                if (target != null)
                {
                    BaseProjectile projectile = ObjectPoolingEverything.GetFromPool<BaseProjectile>(projectileType.ToString());

                    projectile.transform.position = transform.position;

                    projectile.ShootAt(target.position);
                }
            }

            yield return waitForSeconds;
        }
    }

    public Transform FindNearestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, targetLayer);

        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = hit.transform;
            }
        }

        return nearest;
    }
}
