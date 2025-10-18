using UnityEngine;

namespace Game.Enemies
{
    public class Boss : BaseEnemy
    {
        public float chaseSpeed = 3.5f;
        public float attackRange = 1.8f;
        public float stepBackDistance = 1.2f;
        public float idleTime = 2f;

        [HideInInspector] public IdleState idleState;
        [HideInInspector] public ChaseState chaseState;
        [HideInInspector] public AttackState attackState;
        [HideInInspector] public StepBackState stepBackState;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 15;
            idleState = new IdleState(this);
            chaseState = new ChaseState(this);
            attackState = new AttackState(this);
            stepBackState = new StepBackState(this);
        }

        public override void ApplyKnockback(Vector2 hitSource, float force = 7f, float duration = 0.3f)
        {
            // no knock back for boss so leave this empty
        }

        public override EnemyState GetInitialState() => idleState;
    }
}

