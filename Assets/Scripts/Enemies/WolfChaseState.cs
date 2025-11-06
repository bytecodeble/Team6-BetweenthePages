using UnityEngine;

namespace Game.Enemies
{
    public class WolfChaseState : EnemyState
    {
        private Wolf wolf;
        private Vector3 lastKnownPlayerPos;

        public WolfChaseState(Wolf wolf) : base(wolf)
        {
            this.wolf = wolf;
        }

        public override void EnterState()
        {
            // capture initial
            if (wolf.player != null) lastKnownPlayerPos = wolf.player.position;
            Debug.Log("Wolf entering Chase State.");
        }

        public override void FixedUpdateState()
        {
            if (wolf.player == null) return;

            // If player enters dead zone -> stop chase and record last known
            if (wolf.IsInHorizontalDeadZone())
            {
                Debug.Log("Player is in dead zone. -> Tracking State");
                lastKnownPlayerPos = wolf.player.position;
                wolf.ChangeState(new WolfTrackingState(wolf, lastKnownPlayerPos));
                return;
            }

            // If obstacle blocks view -> go to tracking
            if (!wolf.CanSeePlayer(wolf.lossRange))
            {
                // store last known and start tracking
                Debug.Log("Lost line of sight. -> Tracking State");
                lastKnownPlayerPos = wolf.player.position;
                wolf.ChangeState(new WolfTrackingState(wolf, lastKnownPlayerPos));
                return;
            }

            // move horizontally toward player's x
            wolf.MoveTowardsX(wolf.player.position.x, wolf.chaseSpeed);
        }

        public override void UpdateState()
        {
            // If player too far (beyond lossRange) stop chasing and go back to patrol
            if (wolf.player == null) { wolf.ChangeState(wolf.GetInitialState()); return; }

            float dist = Vector2.Distance(wolf.transform.position, wolf.player.position);
            if (dist > wolf.lossRange)
            {
                Debug.Log("Player out of loss range. -> Patrol State");
                wolf.ChangeState(new WolfPatrolState(wolf));
            }
        }
    }
}
