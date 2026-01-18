using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class LightningProjectile : BaseProjectile
{
    public LayerMask hitLayerMask;

    public float remaningPower;
    public float startAttackDamage;

    public List<BaseEnemy> hitTargets;

    public override void ShootAt(Vector2 targetPosition, float attackDamage)
    {
        remaningPower = 1;

        hitTargets = new List<BaseEnemy>();

        this.attackDamage = attackDamage;
        startAttackDamage = attackDamage;

        TransitionTo(targetPosition);
    }

    public void TransitionTo(Vector2 startPosition)
    {
        remaningPower -= 0.1f;

        if (remaningPower <= 0)
        {
            return;
        }

        this.attackDamage = Mathf.Max(remaningPower * startAttackDamage, 0);

        Collider2D[] hits = Physics2D.OverlapCircleAll(startPosition, 1, hitLayerMask);

        foreach (var hit in hits)
        {
            BaseEnemy enemy = hit.GetComponent<BaseEnemy>();

            if (enemy != null)
            {
                if (hitTargets.Contains(enemy))
                {
                    continue;
                }
                else
                {
                    hitTargets.Add(enemy);
                }

                enemy.Hit(this);

                ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.lightningFx);

                fx.transform.position = enemy.transform.position;

                fx.Play();

                Tween.Delay(0.2f).OnComplete(() =>
                {
                    TransitionTo(enemy.transform.position);
                });
            }
        }
    }
}
