using UnityEngine;


namespace Game.Enemies
{
    public class AttackState : EnemyState
    {
        private Boss boss;
        private bool hasAttacked;
        private float attackDuration = 1f;
        private float timer;

        public AttackState(Boss boss) : base(boss)
        {
            this.boss = boss;
        }

        public override void EnterState()
        {
            hasAttacked = false;
            timer = attackDuration;
            boss.rb.linearVelocity = Vector2.zero;
            // animation
        }

        public override void UpdateState()
        {
            timer -= Time.deltaTime;
            if (!hasAttacked && timer <= attackDuration * 0.5f)
            {
                Debug.Log("Boss attacks!");
                hasAttacked = true;
            }

            if (timer <= 0)
            {
                boss.ChangeState(boss.stepBackState);
            }
        }
    }

}
