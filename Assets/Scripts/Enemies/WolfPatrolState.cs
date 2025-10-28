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
            float dir = wolf.IsMovingRight() ? 1f : -1f;
            wolf.rb.linearVelocity = new Vector2(dir * wolf.moveSpeed, wolf.rb.linearVelocity.y);

            bool shouldFlip = false;

            // limit-based flip
            if (wolf.IsMovingRight() && wolf.transform.position.x >= wolf.GetRightLimit().position.x)
            {
                shouldFlip = true;
            }
            else if (!wolf.IsMovingRight() && wolf.transform.position.x <= wolf.GetLeftLimit().position.x)
            {
                shouldFlip = true;
            }

            // ground / wall checks (only if still not decided to flip)
            if (!shouldFlip)
            {
                Vector2 groundCheckOrigin = wolf.groundCheck.position;
                RaycastHit2D groundHit = Physics2D.Raycast(groundCheckOrigin, Vector2.down, wolf.groundCheckDistance, wolf.groundLayer);

                Vector2 wallCheckOrigin = wolf.wallCheck.position;
                RaycastHit2D wallHit = Physics2D.Raycast(wallCheckOrigin, Vector2.right * dir, wolf.wallCheckDistance, wolf.groundLayer);

                if (groundHit.collider == null || wallHit.collider != null)
                    shouldFlip = true;

                Debug.DrawRay(groundCheckOrigin, Vector2.down * wolf.groundCheckDistance, Color.green);
                Debug.DrawRay(wallCheckOrigin, Vector2.right * dir * wolf.wallCheckDistance, Color.red);
            }

            if (shouldFlip)
                wolf.Flip();

            // detection: if player is in sight and not in dead zone, switch to chase
            if (wolf.HasLineOfSightToPlayer(wolf.detectionRange) && !wolf.InHorizontalDeadZone())
            {
                wolf.ChangeState(new WolfChaseState(wolf));
            }
        }

        public override void UpdateState()
        {
            // (optional) could play patrol animation here via wolf's animator
        }
    }
}
