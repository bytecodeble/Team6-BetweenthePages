using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    
    public int maxHealth = 5;
    private int currentHealth;

    public List<Image> hearts;       
    public Sprite fullHeart;        
    public Sprite emptyHeart;       

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHearts();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        Destroy(gameObject);
    }
}

