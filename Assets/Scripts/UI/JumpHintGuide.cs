using UnityEngine;

namespace Game.UI
{
    public class JumpHintGuide : MonoBehaviour
    {
        [SerializeField] private GameObject jumpHintUI;
        private void Start()
        {
            if (jumpHintUI != null)
            {
                jumpHintUI.SetActive(false);
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                if (jumpHintUI != null)
                {
                    jumpHintUI.SetActive(true);
                }
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                if (jumpHintUI != null)
                {
                    jumpHintUI.SetActive(false);
                }
            }
        }
    }
}
