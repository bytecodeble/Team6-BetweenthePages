using UnityEngine;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Boss : BaseEnemy
    {
        [Header("Boss Settings")]
        public float chaseSpeed = 3f;
        public float meleeRange = 6f;
        public float restMin = 3f;
        public float restMax = 5f;
        public float jumpChance = 0.5f;
        public float jumpDuration = 1.5f;
        public float jumpPause = 0.5f;
        public float jumpApexHeight = 4f;
        public GameObject attackHitboxPrefab;

        [Header("Debug Gizmos")]
        public Color detectionColor = Color.yellow;
        public Color meleeColor = Color.red;
        public float gizmoYOffset = 2.55f;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 30;
            currentHealth = maxHealth;
            rb = GetComponent<Rigidbody2D>();

            // use BaseEnemy.detectionRange, set it here to avoid hiding warning
            detectionRange = 15f;

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
            currentHealth -= damage;
            Debug.Log($"Boss.TakeDamage: -{damage} HP, current = {currentHealth}");
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected override void Die()
        {
            Debug.Log("Boss.Die: Boss defeated.");
            Destroy(gameObject, 0.5f);
        }

        public bool IsPlayerInRangeFloat(float range)
        {
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
            if (player == null) return;
            float dir = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
            FacePlayer();
        }

        public void StopMovement()
        {
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
