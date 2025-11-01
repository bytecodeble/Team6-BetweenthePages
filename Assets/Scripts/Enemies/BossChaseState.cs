using UnityEngine;

namespace Game.Enemies
{
    public class BossChaseState : EnemyState
    {
        private Boss boss;
        private bool jumpDecisionMade = false;
        private bool willJump = false;

        public BossChaseState(Boss boss) : base(boss)
        {
            this.boss = boss;
        }

        public override void EnterState()
        {
            jumpDecisionMade = false;
            willJump = false;
            Debug.Log("BossChaseState.Enter");
        }

        public override void UpdateState()
        {
            if (boss.player == null)
            {
                boss.ChangeState(boss.GetInitialState());
                return;
            }

            float dist = Vector2.Distance(boss.transform.position, boss.player.position);

            // if player left detection range -> back to patrol
            if (dist > boss.detectionRange)
            {
                Debug.Log("BossChaseState.Update - player lost -> back to Patrol");
                boss.ChangeState(new BossPatrolState(boss));
                return;
            }

            // if we haven't decided whether to jump this chase, decide once
            if (!jumpDecisionMade)
            {
                willJump = Random.value < boss.jumpChance;
                jumpDecisionMade = true;
                Debug.Log($"BossChaseState.Update - jumpDecisionMade = {willJump}");
            }

            // if within melee range -> attack
            if (dist <= boss.meleeRange)
            {
                Debug.Log("BossChaseState.Update - within melee range -> MeleeAttack");
                boss.ChangeState(new BossMeleeState(boss));
                return;
            }

            // if we decided to jump and player within detection (6..detection), trigger jump
            if (willJump && dist > boss.meleeRange && dist <= boss.detectionRange)
            {
                Debug.Log("BossChaseState.Update - willJump true -> JumpAttack");
                boss.ChangeState(new BossJumpState(boss));
                return;
            }

            // otherwise chase player
            boss.MoveTowardsPlayerX();
        }

        public override void ExitState()
        {
            Debug.Log("BossChaseState.Exit");
        }
    }
}
