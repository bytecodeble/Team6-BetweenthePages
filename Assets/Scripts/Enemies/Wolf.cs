using Game.Environment;
using Game.Managers;
using System.Collections;
using UnityEngine;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Wolf : BaseEnemy
    {
        [Header("Patrol Points")]
        public Transform leftLimit;
        public Transform rightLimit;

        [Header("Ground / Wall Checks")]
        public Transform groundCheck;      // origin for down ray to detect cliff
        public Transform wallCheck;        // origin for horizontal ray to detect wall
        public LayerMask groundLayer;
        public float groundCheckDistance = 1.0f;
        public float wallCheckDistance = 0.2f;

        [Header("Detection / Combat")]
        public float chaseSpeed = 3.5f;
        public float meleeRange = 3f;

        // internal movement facing state
        private bool movingRight = true;

        private SpriteRenderer sr;
        private Collider2D col;

        [Header("SoulOrb")]
        [SerializeField]private GameObject soulOrbPrefab;
        protected override void Awake()
        {
            base.Awake();
            detectionRange = 6f;

            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();

            // make sure player reference exists
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p) player = p.transform;
            }
        }

        public override EnemyState GetInitialState()
        {
            return new WolfPatrolState(this);
        }

        public override void TakeDamage(int damage)
        {
            if (IsDead) return; // prevent getting hit after death
            base.TakeDamage(damage);
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
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(1);
            }
            */

            //call DropSoulOrb function which in this script
            DropSoulOrb(1);


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

        // Flip patrol direction and sprite scale.
        public void FlipDirection()
        {
            movingRight = !movingRight;
            Vector3 s = transform.localScale;
            s.x = -s.x;
            transform.localScale = s;
        }

        public bool IsMovingRight() => movingRight;
        public Transform GetLeftLimit() => leftLimit;
        public Transform GetRightLimit() => rightLimit;

        // Returns true if player is within range and not blocked by obstacles.
        
        public bool CanSeePlayer(float range)
        {
            if (player == null) return false;
            return PlayerInSight(range);
        }

        // Move horizontally toward targetX using either chaseSpeed or moveSpeed.
        public void MoveTowardsX(float targetX, float speed)
        {
            float dir = Mathf.Sign(targetX - transform.position.x);
            if (Mathf.Approximately(dir, 0f))
            {
                // keep previous facing if targetX is very close
                dir = transform.localScale.x >= 0 ? 1f : -1f;
            }

            rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

            // face movement direction
            Vector3 s = transform.localScale;
            s.x = (dir > 0) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            transform.localScale = s;
        }

        public void MovePatrol(float speed)
        {
            float dir = IsMovingRight() ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

            // sprite facing
            Vector3 s = transform.localScale;
            s.x = (dir > 0) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            transform.localScale = s;
        }

        public void StopMovement()
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        private void OnDrawGizmosSelected()
        {
            // draw ground and wall check rays plus detection ranges for easier debugging
            Gizmos.color = Color.green;
            if (groundCheck != null)
            {
                Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
            }

            Gizmos.color = Color.red;
            if (wallCheck != null)
            {
                Vector3 dir = transform.localScale.x >= 0 ? Vector3.right : Vector3.left;
                Gizmos.DrawLine(wallCheck.position, wallCheck.position + dir * wallCheckDistance);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, meleeRange);
        }

        //drop a SoulOrb when enemy die
        private void DropSoulOrb(int score)
        {
            if (soulOrbPrefab != null)
            {
                GameObject orb = Instantiate(soulOrbPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                orb.GetComponent<SoulOrb>().SetValue(score);
            }
        }

    }
}
