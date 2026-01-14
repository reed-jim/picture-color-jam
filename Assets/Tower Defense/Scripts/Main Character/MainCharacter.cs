using System.Collections;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    public Transform weapon;
    public Collider2D weaponCollider;
    public SkeletonAnimation skeletonAnimation;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string moveAnimation;

    public float speedMultiply;
    public float attackRadius;
    public LayerMask checkAttackLayerMask;

    private string currentAnimation;

    void Awake()
    {
        JoystickController.controlPlayerEvent += Move;
        JoystickController.stopControlPlayerEvent += Stop;
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
            Collider2D collider2D = Physics2D.OverlapCircle(transform.position, attackRadius, checkAttackLayerMask);

            if (collider2D != null)
            {
                if (collider2D.GetComponent<BaseEnemy>() != null)
                {
                    weaponCollider.enabled = true;

                    Tween.EulerAngles(weapon, new Vector3(0, 0, 0), new Vector3(0, 0, -360), duration: 0.15f).OnComplete(() =>
                    {
                        weaponCollider.enabled = false;
                    });
                }
            }

            yield return waitForSeconds;
        }
    }
}
