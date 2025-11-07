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

        protected override void Awake()
        {
            base.Awake();
            detectionRange = 6f;

            rb = GetComponent<Rigidbody2D>();

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

        /// <summary>
        /// Flip patrol direction and sprite scale.
        /// </summary>
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

        /// <summary>
        /// Returns true if player is within range and not blocked by obstacles.
        /// Uses BaseEnemy.PlayerInSight under the hood.
        /// </summary>
        public bool CanSeePlayer(float range)
        {
            if (player == null) return false;
            return PlayerInSight(range);
        }

        /// <summary>
        /// Move horizontally toward targetX using either chaseSpeed or moveSpeed.
        /// This preserves vertical velocity.
        /// </summary>
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
    }
}
