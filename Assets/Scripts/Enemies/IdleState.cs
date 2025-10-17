using UnityEngine;

namespace Game.Enemies
{
    public class IdleState : EnemyState
    {
        private float timer;
        private Boss boss;

        public IdleState(Boss boss) : base(boss)
        {
            this.boss = boss;
        }

        public override void EnterState()
        {
            timer = boss.idleTime;
            // animation
        }

        public override void UpdateState()
        {
            timer -= Time.deltaTime;

            if (boss.PlayerInSight(boss.detectionRange))
            {
                boss.ChangeState(boss.chaseState);
                return;
            }

            if (timer <= 0)
            {
                boss.ChangeState(boss.chaseState);
            }
        }
    }
}

