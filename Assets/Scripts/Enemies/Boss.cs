using UnityEngine;
using System.Collections;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Boss : BaseEnemy
    {
        //Boss setting
        private float chaseSpeed = 3f;
        public float meleeRange = 4f;
        public float meleeVerticalRange = 2f;

        public float restMin = 3f;
        public float restMax = 5f;

        // jump attack settings
        public float jumpChance = 0.5f;
        public float jumpDuration = 1.5f;
        public float jumpPause = 0.5f;
        public float jumpApexHeight = 4f;

        // limit combo attacks prevent soft lock
        public int maxComboAttacks = 2;
        public int comboAttackCount = 0;
        public Vector3 roomCenterPos = new Vector3(-30, -3, 0);

        private SpriteRenderer sr;
        private Collider2D col;

        public GameObject attackHitboxPrefab;
        public GameObject chargeEffectPrefab;
        [SerializeField] private GameObject redCloak;

        //Debug gizmos
        private Color detectionColor = Color.yellow;
        private Color meleeColor = Color.red;
        private float gizmoYOffset = 2.55f;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 30;
            currentHealth = maxHealth;

            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();

            // use BaseEnemy.detectionRange, set it here to avoid hiding warning
            detectionRange = 10f;

            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
            }
            else
            {
                Debug.LogError("FATAL: Boss cannot find player object with tag 'Player' in scene!");
            }
        }

        public override EnemyState GetInitialState()
        {
            return new BossPatrolState(this);
        }

        // Boss should not be knocked back, override to do nothing
        public override void ApplyKnockback(Vector2 hitSource, float force = 5f, float duration = 0.2f)
        {
            // intentionally empty 
        }

        public override void TakeDamage(int damage)
        {
            if (IsDead) return;

            currentHealth -= damage;
            if (currentHealth > 0) StartCoroutine(FlashWhite());

            Debug.Log($"Boss.TakeDamage: -{damage} HP, current = {currentHealth}");

            if (currentHealth <= 0) Die();
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

            Debug.Log("Boss.Die: Boss defeated.");

            if (col != null)
                col.enabled = false;
            if (rb != null)
            {
                rb.simulated = false;
                rb.linearVelocity = Vector2.zero;
            }

            StopAllCoroutines();
            StartCoroutine(FadeAndDestroy());

            Vector3 cloakSpawnPos = new Vector3(-30, -0.5f, 0);
            if (redCloak != null)
            {
                Instantiate(redCloak, cloakSpawnPos, Quaternion.identity);
            }

        }

        private IEnumerator FadeAndDestroy()
        {
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

        public bool IsPlayerInRangeFloat(float range)
        {
            if (IsDead) return false;

            if (player == null)
            {
                Debug.LogError("FATAL ERROR: Boss's player reference is NULL! Cannot calculate distance.");
                return false;
            }
            float dist = Vector2.Distance(transform.position, player.position);
            Debug.Log($"Distance Check: Boss Pos={transform.position}, Player Pos={player.position}, Calculated Dist={dist:F2}, Range={range}");
            return Vector2.Distance(transform.position, player.position) <= range;
        }

        public void MoveTowardsPlayerX()
        {
            if (IsDead || player == null) return;
            float dir = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
            FacePlayer();
        }

        public void StopMovement()
        {
            if (IsDead) return;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 gizmoCenter = transform.position + Vector3.up * gizmoYOffset;

            Gizmos.color = detectionColor;
            Gizmos.DrawWireSphere(gizmoCenter, detectionRange);
            
            Gizmos.color = meleeColor;
            Gizmos.DrawWireSphere(gizmoCenter, meleeRange);
        }
    }
}
