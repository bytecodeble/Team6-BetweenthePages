using Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Managers
{
    public class SaveManager : MonoBehaviour
    {
        //UI for hint to savePoint
        [SerializeField] private GameObject saveHintUI;

        [Header("Save Settings")]
        [SerializeField] private KeyCode saveKey = KeyCode.E;
        [SerializeField] private GameObject saveEffectPrefab;

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
                // Instantiate the save effect if it's assigned
                if (saveEffectPrefab != null)
                {
                    GameObject effectInstance = Instantiate(saveEffectPrefab, transform.position, Quaternion.identity);
                    Destroy(effectInstance, 2f);
                }

                // Record player's world position, current scene name, and this save object's name
                Vector3 pos = player.transform.position;
                string sceneName = SceneManager.GetActiveScene().name;
                string spawnObjectName = this.gameObject.name;

                //Record the player's save coordinates,and send values to GameManager
                GameManager.Instance.SetSavePoint(pos, sceneName, spawnObjectName);

                //Recover health when save is activated
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.RestoreFullHealth();
                }

                Debug.Log($"Player saved at: {pos} in scene '{sceneName}' (object '{spawnObjectName}'). Health restored.");
            }
        }

        //The player enters the save point range and the prompt UI appears
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerInRange = true;

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

                if (saveHintUI != null)
                {
                    saveHintUI.SetActive(false);
                }
            }
        }
    }
}
