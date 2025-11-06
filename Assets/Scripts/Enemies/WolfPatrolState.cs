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

        public override void EnterState()
        {
            Debug.Log("Wolf entering Patrol State.");
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
                Vector2 forwardDir = new Vector2(dir, 0);

                // Check for ledge ahead
                if (!Physics2D.Raycast(wolf.groundCheck.position, Vector2.down, wolf.groundCheckDistance, wolf.groundLayer))
                {
                    shouldFlip = true;
                }

                // Check for wall ahead
                if (Physics2D.Raycast(wolf.wallCheck.position, forwardDir, wolf.wallCheckDistance, wolf.groundLayer))
                {
                    shouldFlip = true;
                }
            }

            if (shouldFlip)
                wolf.FlipDirection();
        }

        public override void UpdateState()
        {
            // detection: if can see player and not in unreachable dead zone -> switch to chase
            if (wolf.CanSeePlayer(wolf.detectionRangeClose) && !wolf.IsInHorizontalDeadZone())
            {
                Debug.Log("Player spotted! -> Chase State");
                wolf.ChangeState(new WolfChaseState(wolf));
            }
            else if (wolf.CanSeePlayer(wolf.detectionRangeClose) && wolf.IsInHorizontalDeadZone())
            {
                // intentionally do nothing: player is detected but unreachable
            }
        }
    }
}
