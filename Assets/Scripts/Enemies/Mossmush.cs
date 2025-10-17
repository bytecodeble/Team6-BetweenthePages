using UnityEngine;

namespace Game.Enemies
{
    public class Mossmush : BaseEnemy
    {
        //raycast
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
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        public bool IsMovingRight() => movingRight;
    }
}
