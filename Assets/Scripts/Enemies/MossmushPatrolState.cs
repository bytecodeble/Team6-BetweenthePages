using UnityEngine;

namespace Game.Enemies
{
    public class MossmushPatrolState : EnemyState
    {
        private Mossmush mossmush;

        public MossmushPatrolState(Mossmush mossmush) : base(mossmush)
        {
            this.mossmush = mossmush;
        }

        public override void FixedUpdateState()
        {
            float dir = mossmush.IsMovingRight() ? 1f : -1f;
            mossmush.rb.linearVelocity = new Vector2(dir * mossmush.moveSpeed, mossmush.rb.linearVelocity.y);

            bool shouldFlip = false;

            // limit
            if (mossmush.IsMovingRight() && mossmush.transform.position.x >= mossmush.GetRightLimit().position.x)
            {
                shouldFlip = true;
            }
            else if (!mossmush.IsMovingRight() && mossmush.transform.position.x <= mossmush.GetLeftLimit().position.x)
            {
                shouldFlip = true;
            }

            // raycast
            if (!shouldFlip) 
            {
                Vector2 groundCheckOrigin = mossmush.groundCheck.position;
                RaycastHit2D groundHit = Physics2D.Raycast(groundCheckOrigin, Vector2.down, mossmush.groundCheckDistance, mossmush.groundLayer);

                Vector2 wallCheckOrigin = mossmush.wallCheck.position;
                RaycastHit2D wallHit = Physics2D.Raycast(wallCheckOrigin, Vector2.right * dir, mossmush.wallCheckDistance, mossmush.groundLayer);

                if (groundHit.collider == null || wallHit.collider != null)
                    shouldFlip = true;

                // visual ray
                Debug.DrawRay(groundCheckOrigin, Vector2.down * mossmush.groundCheckDistance, Color.green);
                Debug.DrawRay(wallCheckOrigin, Vector2.right * dir * mossmush.wallCheckDistance, Color.red);

            }

            if (shouldFlip)
                mossmush.Flip();
        }
    }
}
