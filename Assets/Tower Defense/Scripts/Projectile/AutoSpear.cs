using System.Collections;
using Spine;
using UnityEngine;

public class AutoSpear : BaseTower
{
    public Vector3 rotateSpeed;

    public Collider2D bladeCollider;

    void Awake()
    {
        bladeCollider.enabled = false;
    }

    protected override IEnumerator AutoShooting()
    {
        TrackEntry trackEntry;

        while (true)
        {
            if (isActive)
            {
                // Transform target = FindNearestTarget();

                // if (target != null)
                // {
                //     bladeCollider.enabled = true;

                //     trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, attackAnimation, false);

                //     SoundManager.Instance.PlaySound(fireSound);

                //     trackEntry.Complete += OnAttackAnimationComplete;
                // }

                bladeCollider.enabled = true;

                trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, attackAnimation, false);

                SoundManager.Instance.PlaySound(fireSound);

                trackEntry.Complete += OnAttackAnimationComplete;
            }

            yield return new WaitForSeconds(1f / towerProperty.rateOfFire);
        }

        void OnAttackAnimationComplete(TrackEntry trackEntry)
        {
            trackEntry.Complete -= OnAttackAnimationComplete;

            bladeCollider.enabled = false;
        }
    }
}
