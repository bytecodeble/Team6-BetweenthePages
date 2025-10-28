using UnityEngine;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Wolf : BaseEnemy
    {
        [Header("Patrol")]
        public Transform leftLimit;
        public Transform rightLimit;
        public Transform groundCheck;
        public Transform wallCheck;
        public LayerMask groundLayer;
        public float groundCheckDistance = 1.0f;
        public float wallCheckDistance = 0.2f;

        [Header("Detection")]
        public float chaseSpeed = 3.0f;
        public float detectionRangeClose = 6f;   // used for initial detection
        public float lossRange = 8f;             // how far until give up completely
        public float horizontalDeadZone = 1.0f;  // if player within this x delta but too high, stop chase
        public float verticalReach = 0.6f;       // how high relative to enemy the player can be for chase to be valid

        private bool movingRight = true;

        protected override void Awake()
        {
            base.Awake();
            // ensure player is assigned
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p) player = p.transform;
            }
        }

        protected override void Update()
        {
            base.Update();

            // Flip sprite based on movement direction
            if (rb.linearVelocity.x > 0.1f)
                transform.localScale = new Vector3(1, 1, 1);
            else if (rb.linearVelocity.x < -0.1f)
                transform.localScale = new Vector3(-1, 1, 1);

            DrawDebugRays();
        }

        private void DrawDebugRays()
        {
            // Player detection ray
            Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            Vector2 origin = transform.position;
            float detectionDistance = 8f; // whatever your detection range is

            Debug.DrawRay(origin, direction * detectionDistance, Color.yellow);

            // Wall detection ray
            if (wallCheck != null)
                Debug.DrawRay(wallCheck.position, direction * groundCheckDistance, Color.red);

            // Ground check ray
            if (groundCheck != null)
                Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, Color.green);
        }


        public override EnemyState GetInitialState()
        {
            return new WolfPatrolState(this);
        }

        public void FlipDirection()
        {
            movingRight = !movingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }

        public bool IsMovingRight() => movingRight;
        public Transform GetLeftLimit() => leftLimit;
        public Transform GetRightLimit() => rightLimit;

        // returns true if player is within given range AND visible (no obstacles)
        public bool CanSeePlayer(float range)
        {
            if (player == null) return false;
            Vector2 dir = player.position - transform.position;
            float dist = dir.magnitude;
            if (dist > range) return false;

            // check obstacle blocking
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, dist, obstacleLayer);
            if (hit.collider != null) return false;

            // ensure it's actually the player collider (small overlap)
            return Physics2D.OverlapCircle(player.position, 0.3f, playerLayer) != null;
        }

        // Dead zone test: true if player is vertically out of reach while horizontally very close.
        // If true => unreachable (stop chasing)
        public bool IsInHorizontalDeadZone()
        {
            if (player == null) return false;
            float dx = Mathf.Abs(player.position.x - transform.position.x);
            float dy = player.position.y - transform.position.y; // positive = player above
            // If player is horizontally close but vertically out of reach (higher than verticalReach)
            return dx <= horizontalDeadZone && Mathf.Abs(dy) > verticalReach;
        }

        // helper to set chase-facing and movement speed
        public void MoveTowardsX(float targetX, float speed)
        {
            float dir = Mathf.Sign(targetX - transform.position.x);
            if (Mathf.Approximately(dir, 0f)) dir = transform.localScale.x >= 0 ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

            // face direction
            Vector3 s = transform.localScale;
            s.x = (dir > 0) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            transform.localScale = s;
        }

        // optional visualization
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRangeClose);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, lossRange);

            // dead zone rectangle visualization
            Gizmos.color = Color.cyan;
            Vector3 center = transform.position + Vector3.right * (transform.localScale.x >= 0 ? horizontalDeadZone * 0.5f : -horizontalDeadZone * 0.5f);
            Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y + verticalReach * 0.5f, transform.position.z),
                                new Vector3(horizontalDeadZone, verticalReach * 2f, 0.1f));
        }
    }
}
