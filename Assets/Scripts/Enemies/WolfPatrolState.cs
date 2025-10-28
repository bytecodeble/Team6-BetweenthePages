using UnityEngine;

namespace Game.Enemies
{
    public class WolfPatrolState : EnemyState
    {
        private Wolf wolf;

        public WolfPatrolState(Wolf wolf) : base(wolf)
        {
            this.wolf = wolf;
        }

        public override void FixedUpdateState()
        {
            // simple left/right patrol
            float dir = wolf.IsMovingRight() ? 1f : -1f;
            wolf.rb.linearVelocity = new Vector2(dir * wolf.moveSpeed, wolf.rb.linearVelocity.y);

            bool shouldFlip = false;

            // limits
            if (wolf.IsMovingRight() && wolf.transform.position.x >= wolf.GetRightLimit().position.x)
                shouldFlip = true;
            else if (!wolf.IsMovingRight() && wolf.transform.position.x <= wolf.GetLeftLimit().position.x)
                shouldFlip = true;

            // rays for ground & wall like Mossmush
            if (!shouldFlip)
            {
                Vector2 groundOrigin = wolf.groundCheck.position;
                RaycastHit2D groundHit = Physics2D.Raycast(groundOrigin, Vector2.down, wolf.groundCheckDistance, wolf.groundLayer);

                Vector2 wallOrigin = wolf.wallCheck.position;
                RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, Vector2.right * dir, wolf.wallCheckDistance, wolf.groundLayer);

                if (groundHit.collider == null || wallHit.collider != null)
                    shouldFlip = true;

                Debug.DrawRay(groundOrigin, Vector2.down * wolf.groundCheckDistance, Color.green);
                Debug.DrawRay(wallOrigin, Vector2.right * dir * wolf.wallCheckDistance, Color.red);
            }

            if (shouldFlip)
                wolf.FlipDirection();
        }

        public override void UpdateState()
        {
            // detection: if can see player and not in unreachable dead zone -> switch to chase
            if (wolf.CanSeePlayer(wolf.detectionRangeClose) && !wolf.IsInHorizontalDeadZone())
            {
                wolf.ChangeState(new WolfChaseState(wolf));
            }
            else if (wolf.CanSeePlayer(wolf.detectionRangeClose) && wolf.IsInHorizontalDeadZone())
            {
                // intentionally do nothing: player is detected but unreachable
                // optionally we could play a sniff/alert animation here
            }
        }
    }
}
