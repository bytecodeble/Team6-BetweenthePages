using Game.Enemies;
using UnityEngine;

namespace Game.Enemies
{
    public class StepBackState : EnemyState
    {
        private Boss boss;
        private float duration = 0.6f;
        private float timer;

        public StepBackState(Boss boss) : base(boss)
        {
            this.boss = boss;
        }

        public override void EnterState()
        {
            timer = duration;
            boss.FacePlayer();
            Vector2 dir = (boss.player.position.x > boss.transform.position.x) ? Vector2.left : Vector2.right;
            boss.rb.linearVelocity = dir * boss.moveSpeed * 2f;
        }

        public override void UpdateState()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                boss.ChangeState(boss.idleState);
            }
        }
    }
}


