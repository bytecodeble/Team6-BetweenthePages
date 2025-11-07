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
            Debug.Log("WolfChaseState.Enter");
        }

        public override void UpdateState()
        {
            if (wolf.player == null)
            {
                Debug.LogWarning("WolfChaseState.Update - player missing, returning to Patrol");
                wolf.ChangeState(wolf.GetInitialState());
                return;
            }

            // if player is no longer visible within detection range, return to patrol
            if (!wolf.CanSeePlayer(wolf.detectionRange))
            {
                Debug.Log("WolfChaseState.Update - lost sight of player -> returning to Patrol");
                wolf.ChangeState(new WolfPatrolState(wolf));
                return;
            }

            float distance = Vector2.Distance(wolf.transform.position, wolf.player.position);

            // if close enough and still visible, start attack
            if (distance <= wolf.meleeRange && wolf.PlayerInSight(wolf.meleeRange))
            {
                Debug.Log("WolfChaseState.Update - within melee range -> Attack");
                wolf.ChangeState(new WolfAttackState(wolf));
                return;
            }

            // otherwise chase horizontally towards player's x
            wolf.MoveTowardsX(wolf.player.position.x, wolf.chaseSpeed);
        }

        public override void ExitState()
        {
            Debug.Log("WolfChaseState.Exit");
        }
    }
}
