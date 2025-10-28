using UnityEngine;

namespace Game.Enemies
{
    public class WolfChaseState : EnemyState
    {
        private Wolf wolf;

        public WolfChaseState(Wolf wolf) : base(wolf)
        {
            this.wolf = wolf;
        }

        public override void EnterState()
        {
            // immediate face player
            wolf.FacePlayer();
        }

        public override void UpdateState()
        {
            // If player lost line-of-sight, out of range, or in dead zone -> stop chasing
            if (wolf.player == null)
            {
                wolf.ChangeState(new WolfPatrolState(wolf));
                return;
            }

            float dist = Vector2.Distance(wolf.transform.position, wolf.player.position);

            // out of detection range
            if (dist > wolf.detectionRange)
            {
                wolf.ChangeState(new WolfPatrolState(wolf));
                return;
            }

            // blocked by obstacle
            RaycastHit2D hit = Physics2D.Raycast(wolf.transform.position, (wolf.player.position - wolf.transform.position).normalized, dist, wolf.obstacleLayer);
            if (hit.collider != null)
            {
                wolf.ChangeState(new WolfPatrolState(wolf));
                return;
            }

            // horizontal dead zone check (can't reach)
            if (wolf.InHorizontalDeadZone())
            {
                wolf.ChangeState(new WolfPatrolState(wolf));
                return;
            }

            // else, keep chasing horizontally
            wolf.FacePlayer();
        }

        public override void FixedUpdateState()
        {
            if (wolf.player == null) return;

            // move only horizontally toward player (wolf can't jump)
            float sign = Mathf.Sign(wolf.player.position.x - wolf.transform.position.x);
            wolf.rb.linearVelocity = new Vector2(sign * wolf.moveSpeed, wolf.rb.linearVelocity.y);
        }

        public override void ExitState()
        {
            // stop horizontal movement smoothly when leaving chase (keep y velocity)
            wolf.rb.linearVelocity = new Vector2(0f, wolf.rb.linearVelocity.y);
        }
    }
}
