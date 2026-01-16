using Spine.Unity;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BaseEnemy : MonoBehaviour
{
    public Transform destination;

    public SkeletonAnimation skeletonAnimation;
    public LazyHealthBar lazyHealthBar;

    public float attackDamage;
    public float health;
    public float currentHealth;

    [SpineAnimation] public string moveAnimation;

    void Start()
    {
        var agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        currentHealth = health;
    }

    public void Reset()
    {
        currentHealth = health;

        lazyHealthBar.Reset();
    }

    public void MoveToBase()
    {
        GetComponent<NavMeshAgent>().destination = destination.position;

        skeletonAnimation.AnimationState.SetAnimation(0, moveAnimation, true);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        BaseProjectile projectile = collision.transform.GetComponent<BaseProjectile>();

        if (projectile != null)
        {
            Hit(projectile);
        }

        WeaponCollider weaponCollider = collision.transform.GetComponent<WeaponCollider>();

        if (weaponCollider != null)
        {
            Hit(weaponCollider);
        }
    }

    public void Hit(BaseProjectile projectile)
    {
        currentHealth -= projectile.attackDamage;

        lazyHealthBar.ChangeValue(currentHealth / health);

        PlayHitFx();
        ShowDamageText(projectile.attackDamage);

        if (currentHealth <= 0)
        {
            Die();
        }

        if (projectile is not ExplosiveProjectile)
        {
            projectile.gameObject.SetActive(false);
        }
    }

    public void Hit(WeaponCollider weaponCollider)
    {
        currentHealth -= weaponCollider.attackDamage;

        lazyHealthBar.ChangeValue(currentHealth / health);

        PlayHitFx();
        ShowDamageText(weaponCollider.attackDamage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.blockMergeFx);

        fx.transform.position = transform.position;

        fx.Play();

        gameObject.SetActive(false);

        CharacterUI.Instance.UpdateEXP(1);

        GamePersistentVariable.totalEnemy--;

        EnemyManager.Instance.UpdateTotalEnemyCount();
    }

    public void PlayHitFx()
    {
        ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.hitFx);

        fx.transform.position = transform.position;

        fx.Play();

        SoundManager.Instance.PlaySoundHit();
    }

    public void ShowDamageText(float damage)
    {
        DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

        damageText.ShowText(damage, transform.position + new Vector3(0, 1, 0));
    }
}
