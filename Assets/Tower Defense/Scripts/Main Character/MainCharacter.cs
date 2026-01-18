using System.Collections;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    public Transform weapon;
    public Transform shotPosition;
    public Collider2D weaponCollider;
    public SkeletonAnimation skeletonAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string moveAnimation;
    [SpineAnimation] public string attackAnimation;

    public float speedMultiply;
    public LayerMask checkAttackLayerMask;

    private string currentAnimation;
    private Vector3 weaponInitialPosition;
    private Vector3 weaponInitialAngle;

    void Awake()
    {
        JoystickController.controlPlayerEvent += Move;
        JoystickController.stopControlPlayerEvent += Stop;

        weaponInitialPosition = weaponCollider.transform.localPosition;
        weaponInitialAngle = weaponCollider.transform.localEulerAngles;
    }

    void OnDestroy()
    {
        JoystickController.controlPlayerEvent -= Move;
        JoystickController.stopControlPlayerEvent -= Stop;
    }

    void Start()
    {
        StartCoroutine(AttackPeriodically());
    }

    public void Move(Vector2 normalizedDirection)
    {
        transform.position += speedMultiply * (Vector3)normalizedDirection;

        if (normalizedDirection.x > 0)
        {
            transform.localScale = transform.localScale.ChangeX(1);
        }
        else
        {
            transform.localScale = transform.localScale.ChangeX(-1);
        }

        PlayAnimation(moveAnimation);
    }

    public void Stop()
    {
        PlayAnimation(idleAnimation);
    }

    void PlayAnimation(string animationName)
    {
        if (currentAnimation == animationName)
            return;

        skeletonAnimation.AnimationState.SetAnimation(0, animationName, true);
        currentAnimation = animationName;
    }

    private IEnumerator AttackPeriodically()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        weaponCollider.enabled = false;

        while (true)
        {
            Collider2D collider2D = Physics2D.OverlapCircle(transform.position, GamePersistentVariable.mainCharacterStat.attackRadius, checkAttackLayerMask);

            if (collider2D != null)
            {
                if (collider2D.GetComponent<BaseEnemy>() != null)
                {
                    PlayAnimation(attackAnimation);

                    Tween.Delay(0.8f).OnComplete(() =>
                    {
                        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, GamePersistentVariable.mainCharacterStat.attackRadius, checkAttackLayerMask);

                        for (int i = 0; i < hits.Length; i++)
                        {
                            BaseEnemy enemy = hits[i].GetComponent<BaseEnemy>();

                            if (enemy != null)
                            {
                                enemy.Hit(GamePersistentVariable.mainCharacterStat.damage);
                            }
                        }
                    });

                    //weaponCollider.enabled = true;

                    // Tween.EulerAngles(weapon, new Vector3(0, 0, 0), new Vector3(0, 0, -360), duration: 0.15f).OnComplete(() =>
                    // {
                    //     weaponCollider.transform.localPosition = weaponInitialPosition;
                    //     weaponCollider.transform.localEulerAngles = weaponInitialAngle;

                    //     weaponCollider.enabled = false;
                    // });
                }
            }

            // Transform target = FindNearestTarget();

            // if (target != null)
            // {
            //     BaseProjectile projectile = ObjectPoolingEverything.GetFromPool<BaseProjectile>(ProjectileType.Normal.ToString());

            //     projectile.transform.position = shotPosition.position;
            //     projectile.transform.localScale = 0.5f * Vector3.one;

            //     projectile.ShootAt(target.position, GamePersistentVariable.mainCharacterStat.damage);

            //     SoundManager.Instance.PlaySound(GameConstants.balistaSound);
            // }

            yield return new WaitForSeconds(1 / GamePersistentVariable.mainCharacterStat.attackSpeed);
        }
    }

    public Transform FindNearestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, GamePersistentVariable.mainCharacterStat.attackRadius, checkAttackLayerMask);

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

public class MainCharacterStat
{
    public float damage;
    public float attackSpeed;
    public float attackRadius;
}
