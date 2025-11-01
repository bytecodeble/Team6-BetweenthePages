using UnityEngine;

namespace Game.Enemies
{
    public class BossAttackHitbox : MonoBehaviour
    {
        public int damage = 1;
        public float lifetime = 0.3f;

        private void Awake()
        {
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var playerHealth = collision.GetComponent<Game.Player.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("Boss hit player for " + damage + " damage.");
            }
        }
    }
}
