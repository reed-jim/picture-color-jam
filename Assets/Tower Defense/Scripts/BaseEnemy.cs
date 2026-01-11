using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BaseEnemy : MonoBehaviour
{
    public Transform destination;

    public LazyHealthBar lazyHealthBar;

    public float attackDamage;
    public float health;
    public float currentHealth;

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

        lazyHealthBar.ChangeValue(currentHealth / health);
    }

    public void MoveToBase()
    {
        GetComponent<NavMeshAgent>().destination = destination.position;
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

        if (currentHealth <= 0)
        {
            ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.blockMergeFx);

            fx.transform.position = transform.position;

            fx.Play();

            gameObject.SetActive(false);

            CharacterUI.Instance.UpdateEXP(1);
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

        if (currentHealth <= 0)
        {
            ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.blockMergeFx);

            fx.transform.position = transform.position;

            fx.Play();

            gameObject.SetActive(false);

            CharacterUI.Instance.UpdateEXP(1);
        }
    }
}
