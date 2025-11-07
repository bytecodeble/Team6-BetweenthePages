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
            Debug.Log("WolfPatrolState.Enter");
        }

        public override void FixedUpdateState()
        {
            // basic left/right patrol movement
            wolf.MovePatrol(wolf.moveSpeed);

            bool shouldFlip = false;
            // flip when reaching the patrol limits
            if (wolf.IsMovingRight() && wolf.transform.position.x >= wolf.GetRightLimit().position.x)
            {
                shouldFlip = true;
            }
            else if (!wolf.IsMovingRight() && wolf.transform.position.x <= wolf.GetLeftLimit().position.x)
            {
                shouldFlip = true;
            }

            // check for cliff (no ground ahead)
            if (!shouldFlip && wolf.groundCheck != null)
            {
                Vector2 groundOrigin = wolf.groundCheck.position;
                RaycastHit2D groundHit = Physics2D.Raycast(groundOrigin, Vector2.down, wolf.groundCheckDistance, wolf.groundLayer);
                Debug.DrawRay(groundOrigin, Vector2.down * wolf.groundCheckDistance, Color.green);

                if (groundHit.collider == null)
                {
                    shouldFlip = true;
                }
            }

            // check for wall ahead
            if (!shouldFlip && wolf.wallCheck != null)
            {
                Vector2 wallOrigin = wolf.wallCheck.position;
                Vector2 dir = wolf.IsMovingRight() ? Vector2.right : Vector2.left;
                RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, dir, wolf.wallCheckDistance, wolf.groundLayer);
                Debug.DrawRay(wallOrigin, dir * wolf.wallCheckDistance, Color.red);

                if (wallHit.collider != null)
                {
                    shouldFlip = true;
                }
            }

            if (shouldFlip)
            {
                wolf.FlipDirection();
            }
        }

        public override void UpdateState()
        {
            // constantly check for player in sight
            if (wolf.CanSeePlayer(wolf.detectionRange))
            {
                Debug.Log("WolfPatrolState.Update - player detected -> switching to Chase");
                wolf.ChangeState(new WolfChaseState(wolf));
            }
        }

        public override void ExitState()
        {
            Debug.Log("WolfPatrolState.Exit");
        }
    }
}
