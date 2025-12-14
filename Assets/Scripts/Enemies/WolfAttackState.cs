using UnityEngine;

namespace Game.Enemies
{
    public class WolfAttackState : EnemyState
    {
        private Wolf wolf;

        // timings (as requested)
        private float windup = 0.3f;      // pause before dash
        private float dashDuration = 0.2f; // actual dash time (tunable)
        private float recovery = 0.5f;   // recovery after dash

        private enum Phase { Windup, Dash, Recovery }
        private Phase phase;
        private float timer;
        private Vector2 dashTarget;
        private Vector2 dashVelocity; // applied during dash

        public WolfAttackState(Wolf wolf) : base(wolf)
        {
            this.wolf = wolf;
        }

        public override void EnterState()
        {
            timer = 0f;
            phase = Phase.Windup;
            wolf.StopMovement();
            Debug.Log("WolfAttackState.Enter - windup begins");
        }

        public override void UpdateState()
        {
            if (wolf.player == null)
            {
                Debug.LogWarning("WolfAttackState.Update - player missing -> return to Patrol");
                wolf.ChangeState(wolf.GetInitialState());
                return;
            }

            timer += Time.deltaTime;

            if (phase == Phase.Windup)
            {
                // remain paused for windup
                if (timer >= windup)
                {
                    // capture player's current position once (not updated during dash)
                    dashTarget = wolf.player.position;
                    Vector2 startPos = wolf.transform.position;
                    Vector2 dir = (dashTarget - startPos);
                    float distance = dir.magnitude;

                    // avoid zero distance
                    if (distance < 0.01f) distance = 0.01f;

                    // compute required velocity to reach dashTarget in dashDuration
                    dashVelocity = dir.normalized * (distance / Mathf.Max(0.001f, dashDuration));

                    // set face direction to target
                    Vector3 s = wolf.transform.localScale;
                    s.x = (dashTarget.x > wolf.transform.position.x) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
                    wolf.transform.localScale = s;

                    // switch to dash phase
                    phase = Phase.Dash;
                    timer = 0f;
                    Debug.Log($"WolfAttackState.Update - dash started toward {dashTarget} with velocity {dashVelocity}");
                }
            }
            else if (phase == Phase.Dash)
            {
                // dash handled in FixedUpdate (physics). Here check for end conditions.
                // end dash when timer >= dashDuration or close to target
                if (timer >= dashDuration || Vector2.Distance(wolf.transform.position, dashTarget) <= 0.2f)
                {
                    // stop movement
                    wolf.rb.linearVelocity = Vector2.zero;
                    phase = Phase.Recovery;
                    timer = 0f;
                    Debug.Log("WolfAttackState.Update - dash ended, entering recovery");
                }
            }
            else if (phase == Phase.Recovery)
            {
                if (timer >= recovery)
                {
                    // after recovery, pick next state depending on whether player is visible
                    if (wolf.CanSeePlayer(wolf.detectionRange))
                    {
                        Debug.Log("WolfAttackState.Update - recovery finished -> player visible -> Chase");
                        wolf.ChangeState(new WolfChaseState(wolf));
                    }
                    else
                    {
                        Debug.Log("WolfAttackState.Update - recovery finished -> player not visible -> Patrol");
                        wolf.ChangeState(new WolfPatrolState(wolf));
                    }
                }
            }
        }

        public override void FixedUpdateState()
        {
            // apply dash velocity during dash phase
            if (phase == Phase.Dash)
            {
                // apply velocity (overwrite horizontal / vertical velocity)
                wolf.rb.linearVelocity = new Vector2(dashVelocity.x, dashVelocity.y);
            }
        }

        public override void ExitState()
        {
            // ensure velocity cleared
            if (wolf != null)
            {
                wolf.rb.linearVelocity = Vector2.zero;
            }
            Debug.Log("WolfAttackState.Exit");
        }
    }
}
