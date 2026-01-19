using System;
using Unity.VisualScripting;
using UnityEngine;

public class HomeBase : MonoBehaviour
{
    public int health;
    public float currentHealth;

    public LazyHealthBar healthBar;

    public static event Action loseLevelEvent;

    void Start()
    {
        currentHealth = health;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        BaseEnemy enemy = collision.transform.GetComponent<BaseEnemy>();

        if (enemy != null)
        {
            currentHealth--;

            healthBar.ChangeValue(currentHealth / health);
            ShowDamageText(1);

            ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.dieFx);

            fx.transform.position = enemy.transform.position;

            fx.Play();

            enemy.gameObject.SetActive(false);

            if (currentHealth <= 0)
            {
                loseLevelEvent?.Invoke();
            }
        }
    }

    public void ShowDamageText(float damage)
    {
        DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

        damageText.gameObject.SetActive(true);

        damageText.ShowText(damage, transform.position + new Vector3(0, 1, 0));
    }
}
