using UnityEngine;

namespace Game.Player
{
    public class AttackHitbox : MonoBehaviour
    {
        public int damage = 1;
        public float lifetime = 0.4f;

        private void Awake()
        {
            Destroy(gameObject, lifetime); //just in case
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var enemy = collision.GetComponent<Game.Enemies.BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
