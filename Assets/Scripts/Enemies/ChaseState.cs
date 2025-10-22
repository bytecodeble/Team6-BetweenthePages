using UnityEngine;

namespace Game.Enemies
{
    public class ChaseState : EnemyState
    {
        private Boss boss;

        public ChaseState(Boss boss) : base(boss)
        {
            this.boss = boss;
        }

        public override void EnterState()
        {
            // animation
        }

        public override void FixedUpdateState()
        {
            if (boss.player == null) return;

            float dir = Mathf.Sign(boss.player.position.x - boss.transform.position.x);
            boss.rb.linearVelocity = new Vector2(dir * boss.chaseSpeed, boss.rb.linearVelocity.y);
            boss.FacePlayer();

            if (boss.PlayerInSight(boss.attackRange))
            {
                boss.ChangeState(boss.attackState);
            }
        }
    }
}
