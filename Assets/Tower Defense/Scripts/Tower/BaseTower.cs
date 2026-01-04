using System.Collections;
using UnityEngine;

public class BaseTower : MonoBehaviour
{
    public float attackRadius;
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
            Transform target = FindNearestTarget();

            if (target != null)
            {
                BaseProjectile projectile = ObjectPoolingEverything.GetFromPool<BaseProjectile>(GameConstants.projectile);

                projectile.transform.position = transform.position;

                projectile.ShootAt(target.position);
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
