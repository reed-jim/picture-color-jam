using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BaseEnemy : MonoBehaviour
{
    [SerializeField] private Transform destination;

    public LazyHealthBar lazyHealthBar;

    public float attackDamage;
    public float health;
    public float currentHealth;

    void Start()
    {
        var agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.destination = destination.position;

        currentHealth = health;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        BaseProjectile projectile = collision.transform.GetComponent<BaseProjectile>();

        if (projectile != null)
        {
            Hit(projectile);
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
        }

        projectile.gameObject.SetActive(false);
    }
}
