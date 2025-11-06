using UnityEngine;

namespace Game.Enemies
{
    public class WolfTrackingState : EnemyState
    {
        private Wolf wolf;
        private Vector3 targetPos;
        private float arriveThreshold = 0.25f;
        private float trackingTimeout = 4f;
        private float timer = 0f;

        public WolfTrackingState(Wolf wolf, Vector3 lastKnown) : base(wolf)
        {
            this.wolf = wolf;
            targetPos = lastKnown;
        }

        public override void EnterState()
        {
            timer = 0f;
            Debug.Log($"Wolf entering Tracking State, moving to {targetPos}.");
        }

        public override void FixedUpdateState()
        {
            // move to last known x position
            wolf.MoveTowardsX(targetPos.x, wolf.moveSpeed * 0.9f);
        }

        public override void UpdateState()
        {
            timer += Time.deltaTime;

            // if in sight again and reachable -> chase
            if (wolf.CanSeePlayer(wolf.detectionRangeClose) && !wolf.IsInHorizontalDeadZone())
            {
                Debug.Log("Player reacquired. -> Chase State");
                wolf.ChangeState(new WolfChaseState(wolf));
                return;
            }

            // if reached last known position or timed out -> return to patrol
            if (Mathf.Abs(wolf.transform.position.x - targetPos.x) <= arriveThreshold || timer >= trackingTimeout)
            {
                Debug.Log("Tracking timed out or destination reached. -> Patrol State");
                wolf.ChangeState(new WolfPatrolState(wolf));
            }
        }
    }
}
