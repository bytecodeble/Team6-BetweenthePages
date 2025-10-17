using UnityEngine;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class BaseEnemy : MonoBehaviour
    {
        public int maxHealth = 3;
        public float moveSpeed = 2f;

        public Transform player;
        public float detectionRange = 6f;
        public LayerMask playerLayer;
        public LayerMask obstacleLayer;

        public Rigidbody2D rb;

        protected int currentHealth;
        protected EnemyState currentState;



        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            currentHealth = maxHealth;
        }

        protected virtual void Start()
        {
            ChangeState(GetInitialState());
        }

        protected virtual void Update()
        {
            currentState?.UpdateState();
        }

        protected virtual void FixedUpdate()
        {
            currentState?.FixedUpdateState();
        }

        public abstract EnemyState GetInitialState();

        public virtual void ChangeState(EnemyState newState)
        {
            if (currentState != null)
                currentState.ExitState();
            currentState = newState;
            currentState.EnterState();
        }

        public virtual void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            Destroy(gameObject, 0.5f);
        }

        public bool PlayerInSight(float range)
        {
            if (player == null) return false;

            Vector2 direction = player.position - transform.position;
            float distance = direction.magnitude;

            if (distance > range) return false;

            // check wall
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, obstacleLayer);
            if (hit.collider != null) return false; // blocked

            // chekc player
            return Physics2D.OverlapCircle(player.position, 0.2f, playerLayer);
        }

        public void FacePlayer()
        {
            if (player == null) return;
            Vector3 scale = transform.localScale;
            scale.x = (player.position.x > transform.position.x) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}
