using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Managers;

namespace Game.Player
{

    public class PlayerHealth : MonoBehaviour
    {
        //setting health value
        [SerializeField] private int maxHealth = 5;
        private int currentHealth;

        //setting UI of health canvas
        [SerializeField] private List<Image> hearts;
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite emptyHeart;

        //setting DamageEffect
        [SerializeField] private float invincibleTime = 1.5f;
        public bool isInvincible = false;

        public delegate void PlayerEvent();
        public event PlayerEvent OnDamageTaken;
        public event PlayerEvent OnDeath;

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
                OnDeath?.Invoke();
                Die();
            }
            else
            {
                OnDamageTaken?.Invoke();
                StartCoroutine(DamageEffect());
            }
        }

        private IEnumerator DamageEffect()
        {
            //when player damaged, start invincible status
            isInvincible = true;

            PlayerControl pc = GetComponent<PlayerControl>();
            if (pc != null)
            {
                pc.StartInvincibleFlicker(invincibleTime);
            }


            //get into invincibale time
            yield return new WaitForSeconds(invincibleTime);

            isInvincible = false;
        }

        //Update health UI
        private void UpdateHearts()
        {
            for (int i = 0; i < hearts.Count; i++)
            {
                hearts[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
            }
        }

        private void Die()
        {
            OnDeath?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartCoroutine(GameManager.Instance.DeathSequence(gameObject));
            }

        }

        public float GetInvincibleTime()
        {
            return invincibleTime;
        }

        public void RestoreFullHealth()
        {
            currentHealth = maxHealth;
            UpdateHearts();
        }
    }


}
