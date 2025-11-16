using UnityEngine;
using Game.Managers;

namespace Game.Environment
{
    public class SoulOrb : MonoBehaviour
    {
        private int scoreValue;

        [SerializeField]private float floatHeight = 0.5f;
        [SerializeField]private float floatSpeed = 1f;
        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;
           
        }

        private void Update()
        {
            // SoulOrb floating in scene
            transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        }

        public void SetValue(int score)
        {
            scoreValue = score;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // player get score when collecting SoulOrb
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(scoreValue);
                }
               
                Destroy(gameObject);
            }
        }
    }
}
