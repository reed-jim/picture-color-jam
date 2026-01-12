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

            enemy.gameObject.SetActive(false);

            if (currentHealth <= 0)
            {
                loseLevelEvent?.Invoke();
            }
        }
    }
}
