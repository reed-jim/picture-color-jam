using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class LightningProjectile : BaseProjectile
{
    public LayerMask hitLayerMask;

    public float remaningPower;
    public float startAttackDamage;

    public List<BaseEnemy> hitTargets;

    // public Transform lightningTrail;


    public override void ShootAt(Vector2 targetPosition, float attackDamage)
    {
        remaningPower = 1;

        hitTargets = new List<BaseEnemy>();

        this.attackDamage = attackDamage;
        startAttackDamage = attackDamage;

        gameObject.SetActive(true);
        transform.position = shooterPosition;

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

        Collider2D[] hits = Physics2D.OverlapCircleAll(startPosition, 2, hitLayerMask);

        bool isFound = false;

        for (int i = 0; i < hits.Length; i++)
        {
            BaseEnemy enemy = hits[i].GetComponent<BaseEnemy>();

            if (enemy != null)
            {
                if (hitTargets.Contains(enemy))
                {
                    continue;
                }
                else
                {
                    hitTargets.Add(enemy);

                    isFound = true;
                }

                // lightningTrail.gameObject.SetActive(true);

                SoundManager.Instance.PlaySoundLightningZap();

                Tween.Position(transform, enemy.transform.position, duration: 0.3f)
                .OnComplete(() =>
                {
                    enemy.Hit(this);

                    TransitionTo(enemy.transform.position);
                });

                break;

                // ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.lightningFx);

                // fx.transform.position = enemy.transform.position;

                // fx.Play();

                // Tween.Delay(0.2f).OnComplete(() =>
                // {
                //     TransitionTo(enemy.transform.position);
                // });
            }
        }

        if (!isFound)
        {
            gameObject.SetActive(false);
        }
    }

    public void TransitionToAll(Vector2 startPosition)
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
