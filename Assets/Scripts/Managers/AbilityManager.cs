using Game.Environment;
using Game.Player;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Managers
{
    public class AbilityManager : MonoBehaviour
    {
        public static AbilityManager Instance;
        private GameObject doubleJumpUIPanel;
        public bool isDoubleJumpUnlocked = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }

            isDoubleJumpUnlocked = false;
        }

        public void InitializePlayerAbilities(PlayerControl player)
        {
            player.hasDoubleJump = isDoubleJumpUnlocked;

            if (isDoubleJumpUnlocked)
            {
                player.maxDoubleJump = 1;
                Debug.Log("[AbilityManager] double jump unlocked. ");
            }
            else
            {
                player.maxDoubleJump = 0;
                Debug.Log("[AbilityManager] double jump locked");
            }
            Debug.Log($"[AbilityManager] isDoubleJumpUnlocked = {isDoubleJumpUnlocked}");
        
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var player = FindFirstObjectByType<PlayerControl>();
            if (player != null)
                InitializePlayerAbilities(player);

            doubleJumpUIPanel = GameObject.Find("doubleJumpUIPanel");
            if (doubleJumpUIPanel != null)
            {
                doubleJumpUIPanel.SetActive(false);
                Debug.Log($"[AbilityManager] Found doubleJumpUIPanel in {scene.name}");
            }
        }

        public void StartDoubleJumpAcquisition(PlayerControl player, RedCloakItem cloak)
        {
            StartCoroutine(DoubleJumpSequence(player, cloak));
        }

        private IEnumerator DoubleJumpSequence(PlayerControl player, RedCloakItem cloak)
        {
            player.LockInput();
            player.IdleAnimation();

            Time.timeScale = 0f;

            if (doubleJumpUIPanel != null)
                doubleJumpUIPanel.SetActive(true);
            else Debug.Log("[AbilityManager] Double jump panel not found!");

                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));


            if (doubleJumpUIPanel != null)
                doubleJumpUIPanel.SetActive(false);

            Time.timeScale = 1f;

            player.hasDoubleJump = true;
            player.maxDoubleJump = 1;
            isDoubleJumpUnlocked = true;
            Debug.Log("[AbilityManager] Double jump unlocked!");

            player.UnlockInput();

            Destroy(cloak.gameObject);
        }
    }
}
