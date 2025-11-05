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

            float radialDist = Vector2.Distance(boss.transform.position, boss.player.position);

            // if player left detection range -> back to patrol
            if (radialDist > boss.detectionRange)
            {
                Debug.Log("BossChaseState.Update - player lost -> back to Patrol");
                boss.ChangeState(new BossPatrolState(boss));
                return;
            }

            // separate distances for state decisions
            float horizontalDist = Mathf.Abs(boss.player.position.x - boss.transform.position.x);
            float verticalDist = Mathf.Abs(boss.player.position.y - boss.transform.position.y);

            // force jump if player x close but y far
            if (horizontalDist <= boss.meleeRange && verticalDist > boss.meleeVerticalRange) 
            {
                Debug.Log("BossChaseState.Update - player on different level, force jump");
                boss.ChangeState(new BossJumpState(boss));
                return;
            }

            // if within new melee range -> attack
            if (horizontalDist <= boss.meleeRange && verticalDist <= boss.meleeVerticalRange)
            {
                Debug.Log("BossChaseState.Update - within melee range -> MeleeAttack");
                boss.ChangeState(new BossMeleeState(boss));
                return;
            }


            // if we haven't decided whether to jump this chase, decide once
            if (!jumpDecisionMade)
            {
                willJump = Random.value < boss.jumpChance;
                jumpDecisionMade = true;
                Debug.Log($"BossChaseState.Update - jumpDecisionMade = {willJump}");
            }


            // if we decided to jump and player is outside melee range, trigger jump
            if (willJump && radialDist > boss.meleeRange)
            {
                Debug.Log("BossChaseState.Update - Jump");
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
