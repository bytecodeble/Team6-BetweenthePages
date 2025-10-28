using UnityEngine;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Wolf : BaseEnemy
    {
        public Transform leftLimit;
        public Transform rightLimit;
        public Transform groundCheck;
        public Transform wallCheck;

        public LayerMask groundLayer;
        public float groundCheckDistance = 1.0f;
        public float wallCheckDistance = 0.2f;

        // Dead zone settings for tracking logic:
        // If the player is vertically further than maxVerticalChase AND horizontally
        // closer than horizontalDeadZone, the wolf gives up the chase (can't jump).
        public float maxVerticalChase = 1.0f;
        public float horizontalDeadZone = 0.5f;

        private bool movingRight = true;

        protected override void Awake()
        {
            base.Awake();
            // tweak health/speed defaults for wolf if you want
            maxHealth = 3;
        }

        public override EnemyState GetInitialState()
        {
            return new WolfPatrolState(this);
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

        // Utility: check line of sight to player using obstacleLayer and distance
        public bool HasLineOfSightToPlayer(float range)
        {
            if (player == null) return false;

            Vector2 dir = player.position - transform.position;
            float dist = dir.magnitude;
            if (dist > range) return false;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, dist, obstacleLayer);
            if (hit.collider != null) return false; // blocked by obstacle

            // sanity: confirm overlapping player layer at player position
            return Physics2D.OverlapCircle(player.position, 0.2f, playerLayer) != null;
        }

        // Check dead zone: if player vertical difference is too large but horizontal is very close
        public bool InHorizontalDeadZone()
        {
            if (player == null) return false;

            float verticalDiff = Mathf.Abs(player.position.y - transform.position.y);
            float horizontalDiff = Mathf.Abs(player.position.x - transform.position.x);

            return (verticalDiff > maxVerticalChase) && (horizontalDiff < horizontalDeadZone);
        }
    }
}
