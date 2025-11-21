using Game.Environment;
using Game.Managers;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Game.Enemies
{
    public class Mossmush : BaseEnemy
    {
        public Transform leftLimit;
        public Transform rightLimit;
        public Transform groundCheck;
        public Transform wallCheck;

        public LayerMask groundLayer;
        public float groundCheckDistance = 1.0f;
        public float wallCheckDistance = 0.2f;

        private SpriteRenderer sr;
        private Collider2D col;

        private bool movingRight = true;

        [SerializeField] private GameObject soulOrbPrefab;
        [SerializeField] private GameObject damageEffect;


        protected override void Awake()
        {
            base.Awake();
            maxHealth = 2;
            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
        }

        public override EnemyState GetInitialState()
        {
            return new MossmushPatrolState(this);
        }

        public void Flip()
        {
            movingRight = !movingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }

        public bool IsMovingRight() => movingRight;
        public Transform GetLeftLimit() => leftLimit;
        public Transform GetRightLimit() => rightLimit;


        public override void TakeDamage(int damage)
        {
            if (IsDead) return; // prevent getting hit after death
            base.TakeDamage(damage);

            //spawn blood splash for mush
            if (damageEffect != null)
            {
                GameObject effect = Instantiate(damageEffect, transform.position, Quaternion.identity);
                Destroy(effect,1f);
            }

            StartCoroutine(FlashWhite());
        }

        private IEnumerator FlashWhite()
        {
            if (sr == null) yield break;

            Color original = sr.color;
            Color flash = Color.grey;

            for (int i = 0; i < 3; i++)
            {
                sr.color = flash;
                yield return new WaitForSeconds(0.05f);
                sr.color = original;
                yield return new WaitForSeconds(0.05f);
            }
            sr.color = original;
        }

        protected override void Die()
        {
            if (IsDead) return;
            IsDead = true;

            /*
            // get score when killed  
            if (ScoreManager.Instance != null) {

                ScoreManager.Instance.AddScore(1);
            }*/

            //call DropSoulOrb function which in this script
            DropSoulOrb(2);


            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            if (col != null)
                col.enabled = false;
            if (rb != null)
                rb.simulated = false;

            if (sr != null)
            {
                Color original = sr.color;
                Color gray = Color.gray;

                float fadeDuration = 0.5f;
                float t = 0f;
                while (t < fadeDuration)
                {
                    sr.color = Color.Lerp(original, gray, t / fadeDuration);
                    t += Time.deltaTime;
                    yield return null;
                }
                sr.color = gray;
            }

            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        //drop a SoulOrb when enemy die
        private void DropSoulOrb(int score)
        {
            if (soulOrbPrefab != null)
            {
                GameObject orb = Instantiate(soulOrbPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
                orb.GetComponent<SoulOrb>().SetValue(score);
            }
        }

    }
}
