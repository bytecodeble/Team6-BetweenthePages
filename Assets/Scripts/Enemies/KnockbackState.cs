using UnityEngine;

namespace Game.Enemies
{
    public class KnockbackState : EnemyState
    {
        private float knockbackDuration;
        private float knockbackTimer;
        private EnemyState previousState;

        public KnockbackState(BaseEnemy enemy, float duration, EnemyState returnState) : base(enemy)
        {
            knockbackDuration = duration;
            previousState = returnState;
        }

        public override void EnterState()
        {
            knockbackTimer = 0f;
        }

        public override void UpdateState()
        {
            knockbackTimer += Time.deltaTime;

            if (knockbackTimer >= knockbackDuration)
            {
                enemy.ChangeState(previousState);
            }
        }

        public override void FixedUpdateState()
        {
            // empty for no velocity override
        }
    }
}
