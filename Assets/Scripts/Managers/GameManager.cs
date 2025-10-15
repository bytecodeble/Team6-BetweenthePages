using Game.Player;
using Game.UI;
using System.Collections;
using UnityEngine;

namespace Game.Managers
{

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform respawnPoint;

        private GameObject currentPlayer;

        private Vector3? savedPosition = null;



        private void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            RespawnPlayer();
        }

        void Update()
        {

        }


        //get position values from SaveManager
        public void SetSavePoint(Vector3 position)
        {
            savedPosition = position;
        }

        public void RespawnPlayer()
        {
            //Make sure the player instance on the field has been destroyed.
            if (currentPlayer != null)
            {
                Destroy(currentPlayer);
            }
            Vector3 spawnPos = savedPosition ?? respawnPoint.position;

            // Spawn a new player Prefab at the respawn point
            currentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }


        public IEnumerator DeathSequence(GameObject deadPlayer)
        {
            float fadeDuration = 0.6f;
            float postRespawnSettle = 0.05f;

            // stop input and play death animation
            PlayerControl pc = deadPlayer.GetComponent<PlayerControl>();
            PlayerHealth ph = deadPlayer.GetComponent<PlayerHealth>();

            if (pc != null)
            {
                pc.LockInput();
                yield return pc.PlayDeathAndWait();
            }
            else
            {
                yield return new WaitForSeconds(0.25f);
            }

            // fade out
            if (ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeOutCoroutine(fadeDuration);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }

            RespawnPlayer();

            // same frame to ensure player exists
            yield return new WaitForSeconds(postRespawnSettle);

            // fade in
            if (ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeInCoroutine(fadeDuration);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }

            // invincibility after respawn and flicker
            if (currentPlayer != null)
            {
                PlayerHealth newPH = currentPlayer.GetComponent<PlayerHealth>();
                PlayerControl newPC = currentPlayer.GetComponent<PlayerControl>();

                float invTime = newPH != null ? newPH.GetInvincibleTime() : 1.5f;

                if (newPH != null)
                {
                    newPH.isInvincible = true;
                }

                if (newPC != null)
                {
                    newPC.LockInput();
                    newPC.StartInvincibleFlicker(invTime);
                }

                yield return new WaitForSeconds(invTime);

                if (newPH != null)
                {
                    newPH.isInvincible = false;
                }

                if (newPC != null)
                {
                    newPC.UnlockInput();
                }


            }


        }

    }
}