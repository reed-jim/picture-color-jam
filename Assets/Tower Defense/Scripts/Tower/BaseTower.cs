using System;
using System.Collections;
using System.Collections.Generic;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using UnityEngine;
using static GameEnum;

public enum ProjectileType
{
    Normal,
    Explosive,
    Lightning
}

public class BaseTower : MonoBehaviour
{
    public SkeletonAnimation skeletonAnimation;
    public Collider2D towerCollider;

    public TowerType towerType;
    public bool isActive;
    public TowerProperty towerProperty;
    public ProjectileType projectileType;
    public LayerMask targetLayer;

    [SpineAnimation] public string attackAnimation;
    public string fireSound;

    void Start()
    {
        towerProperty.damage = towerProperty.baseDamage;
        towerProperty.attackRadius = towerProperty.baseAttackRadius;
        towerProperty.rateOfFire = towerProperty.baseRateOfFire;
    }

    public void StartAutoShooting()
    {
        StartCoroutine(AutoShooting());
    }

    protected virtual IEnumerator AutoShooting()
    {
        while (true)
        {
            if (isActive)
            {
                Transform target = FindNearestTarget();

                if (target != null)
                {
                    BaseProjectile projectile = ObjectPoolingEverything.GetFromPool<BaseProjectile>(projectileType.ToString());

                    projectile.transform.position = transform.position;
                    projectile.transform.localScale = Vector3.one;

                    if (target.position.x > transform.position.x)
                    {
                        transform.localScale = transform.localScale.ChangeX(1);
                    }
                    else
                    {
                        transform.localScale = transform.localScale.ChangeX(-1);
                    }

                    projectile.ShootAt(target.position, towerProperty.damage);

                    SoundManager.Instance.PlaySound(fireSound);

                    skeletonAnimation.AnimationState.SetAnimation(0, attackAnimation, false);
                }
            }

            yield return new WaitForSeconds(1f / towerProperty.rateOfFire);
        }
    }

    public Transform FindNearestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, towerProperty.attackRadius, targetLayer);

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

    public void TryApplyUpgrade(TowerType towerType, float buffRatio)
    {
        if (towerType == this.towerType)
        {
            towerProperty.damage *= buffRatio;

            VFXManager.Instance.PlayBuffFX(transform.position);
        }
    }
}

[Serializable]
public class TowerProperty
{
    public Sprite icon;
    public float baseDamage;
    public float damage;
    public float baseAttackRadius;
    public float attackRadius;
    public float baseRateOfFire;
    public float rateOfFire;
}

public class GlobalTowerUpgrade
{
    public List<TowerUpgrade> towerUpgrades;
}

public struct TowerUpgrade
{
    public float damageRatio;
}
