using UnityEngine;

namespace Game.UI
{
    public class AttackHintGuide : MonoBehaviour
    {
        [SerializeField] private GameObject attackHintUI;

        void Start()
        {
            if (attackHintUI != null)
            {
                attackHintUI.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                if (attackHintUI != null)
                {
                    attackHintUI.SetActive(true);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                if (attackHintUI != null)
                {
                    attackHintUI.SetActive(false);
                }
            }
        }
    }
}
