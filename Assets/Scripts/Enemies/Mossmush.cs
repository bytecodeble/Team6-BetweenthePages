using UnityEngine;

namespace Game.Enemies
{
    public class Mossmush : BaseEnemy
    {
        public Transform leftLimit;
        public Transform rightLimit;
        public Transform groundCheck;
        public Transform wallCheck;

        public LayerMask groundLayer;

        public float groundCheckDistance = 1.0f;
        public float wallCheckDistance = 0.2f;

        private bool movingRight = true;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 2;
        }

        public override EnemyState GetInitialState()
        {
            return new MossmushPatrolState(this);
        }

        public void Flip()
        {
            movingRight = !movingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }

        public bool IsMovingRight() => movingRight;
        public Transform GetLeftLimit() => leftLimit;
        public Transform GetRightLimit() => rightLimit;
    }
}
