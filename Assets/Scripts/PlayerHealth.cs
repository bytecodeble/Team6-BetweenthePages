using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    //setting health value
    [SerializeField]private int maxHealth = 5;
    private int currentHealth;

    //setting UI of health canvas
    [SerializeField] private List<Image> hearts;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    //setting DamageEffect
    [SerializeField] private float flashTime = 0.5f;
    [SerializeField] private float invincibleTime = 2f;

    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();

        spriteRenderer = GetComponent<SpriteRenderer>();

        //record origial color of player
        originalColor = spriteRenderer.color; 
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
        //if player in invincible status,jump out of the TakeDamage()
        if (isInvincible)
        {
            return;
        } 
        //if player is not vincible,go on
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHearts();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageEffect());
        }
    }

    private IEnumerator DamageEffect()
    {
        //when player damaged, start invincible status
        isInvincible = true;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashTime);

        //after flash, color switch to original color
        spriteRenderer.color = originalColor;

        //get into invincibale time
        yield return new WaitForSeconds(invincibleTime);

        isInvincible = false;
    }

    //Update health UI
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
        //When player died. Call GameManager's RespawnPlayer()
        FindFirstObjectByType<GameManager>().RespawnPlayer();
    }
}

