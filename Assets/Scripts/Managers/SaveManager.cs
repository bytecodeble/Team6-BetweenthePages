using Game.Player;
using UnityEngine;

namespace Game.Managers
{
    public class SaveManager : MonoBehaviour
    {
        //UI for hint to savePoint
        [SerializeField] private GameObject saveHintUI;

        [Header("Save Settings")]
        [SerializeField] private KeyCode saveKey = KeyCode.E;

        //Check if player get into savePoint range
        private bool playerInRange = false;
        void Start()
        {
            if (saveHintUI != null)
            {
                //Hidden UI hint by default
                saveHintUI.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (playerInRange && Input.GetKeyDown(saveKey))
            {
                SavePlayerPosition();
            }
        }

        private void SavePlayerPosition()
        {
            //looking for player in scenes
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                //Record the player's save coordinates,and send values to GameManager
                GameManager.Instance.SetSavePoint(player.transform.position);

                //Recover health when save is activated
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.RestoreFullHealth();
                }

                Debug.Log("Player saved at: " + player.transform.position + "Health restored");
            }
        }

        //The player enters the save point range and the prompt UI appears
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerInRange = true;
                Debug.Log("Player entered save range");

                if (saveHintUI != null)
                {
                    saveHintUI.SetActive(true);
                }
            }
        }

        //The player leave the save point range and the prompt UI disappears
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerInRange = false;
                Debug.Log("Player entered save range");

                if (saveHintUI != null)
                {
                    saveHintUI.SetActive(false);
                }
            }
        }
    }
}
