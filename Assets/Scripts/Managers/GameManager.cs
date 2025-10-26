using Game.Player;
using Game.UI;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;

namespace Game.Managers
{

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private GameObject playerPrefab;
        private string defaultSceneName = "TutorialRoom";
        private string defaultRespawnName = "Respawn Point";


        private GameObject currentPlayer;

        private Vector3? savedPosition = null;
        private string savedSceneName = null;
        private string savedSpawnName = null;



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
            SceneManager.LoadScene("MenuScene");
        }

        public void StartNewGame()
        {
            StartCoroutine(RespawnPlayerCoroutine(false));
        }

        //get position values from SaveManager
        public void SetSavePoint(Vector3 position, string sceneName, string spawnObjectName)
        {
            savedPosition = position;
            savedSceneName = sceneName;
            savedSpawnName = spawnObjectName;

            Debug.Log($"[GameManager] Save point stored: {position} @ '{sceneName}' ('{spawnObjectName}')");

        }

        public void RespawnPlayer(bool Invinci = false)
        {
            StartCoroutine(RespawnPlayerCoroutine(Invinci));
        }

        private IEnumerator RespawnPlayerCoroutine(bool Invinci)
        {
            // determine target scene
            string targetSceneName = !string.IsNullOrEmpty(savedSceneName) ? savedSceneName : defaultSceneName;
            Vector3 spawnPos = Vector3.zero;
            bool useSavedPosition = savedPosition.HasValue && savedSceneName == targetSceneName;

            // record current scene
            Scene fromScene = SceneManager.GetActiveScene();

            // destroy existing player instance if any
            if (currentPlayer != null)
            {
                Destroy(currentPlayer);
                currentPlayer = null;
                yield return null; // wait a frame to make sure destoryed
            }

            // load target scene additive
            Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
            if (!targetScene.isLoaded)
            {
                var loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
                if (loadOp == null)
                {
                    Debug.LogError($"[GameManager] LoadSceneAsync returned null for '{targetSceneName}'");
                    yield break;
                }
                yield return new WaitUntil(() => loadOp.isDone);
                targetScene = SceneManager.GetSceneByName(targetSceneName);
                if (!targetScene.isLoaded)
                {
                    Debug.LogError($"[GameManager] Failed to load scene '{targetSceneName}'. Aborting respawn.");
                    yield break;
                }
            }

            // decide spawn pos
            if (useSavedPosition) spawnPos = savedPosition.Value;
            else
            {
                // try to find spawn point in the target scene
                string wantName = !string.IsNullOrEmpty(savedSpawnName) ? savedSpawnName : defaultRespawnName;
                Transform respawnTransform = FindTransformInScene(targetScene, wantName);

                if(respawnTransform != null)
                {
                    spawnPos = respawnTransform.position;
                }
                else
                {
                    if(wantName != defaultRespawnName)
                    {
                        Transform fallback = FindTransformInScene(targetScene, defaultRespawnName);
                        if (fallback != null) spawnPos = fallback.position;
                        else
                        {
                            spawnPos = Vector3.zero;
                            Debug.LogWarning($"[GameManager] respawn object '{wantName}' or '{defaultRespawnName}' not found in '{targetSceneName}'");
                        }
                    }
                    else
                    {
                        spawnPos = Vector3.zero;
                        Debug.LogWarning($"[GameManager] respawn object '{defaultRespawnName}' not found in '{targetSceneName}'");
                    }
                }
            }

            currentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            
            // player move to target scene
            if (currentPlayer != null && targetScene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(currentPlayer, targetScene);

                //Camera Confiner
                if (RoomManager.Instance != null)
                    //StartCoroutine(RoomManager.Instance.BindCameraConfinerWhenReady(targetScene));
                    yield return StartCoroutine(RoomManager.Instance.BindCameraConfinerWhenReady(targetScene));
                
                // Camera follow player
                if (CameraManager.Instance != null)
                    CameraManager.Instance.FollowPlayer(currentPlayer);
               
            }
            
            // set invincibility if requested
            if (Invinci)
            {
                var ph = currentPlayer.GetComponent<PlayerHealth>();
                if (ph != null) ph.isInvincible = true;
            }

            yield return new WaitForSecondsRealtime(0.05f);

            // optionally unload the fromScene if it's a different scene
            Scene bootstrapScene = this.gameObject.scene;
            if (fromScene.isLoaded && fromScene.name != targetSceneName && fromScene.name != bootstrapScene.name)
            {
                var unloadOp = SceneManager.UnloadSceneAsync(fromScene);
                if (unloadOp != null) yield return unloadOp;
            }

            // tiny settle
            yield return new WaitForSecondsRealtime(0.05f);

        }

        public IEnumerator DeathSequence(GameObject deadPlayer)
        {
            float fadeDuration = 1.0f;
            float postRespawnSettle = 0.5f;

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
                yield return new WaitForSeconds(0.4f);
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

            RespawnPlayer(Invinci: true);

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


                if (newPC != null)
                {
                    newPC.LockInput();
                    newPC.StartInvincibleFlicker(invTime);
                    yield return new WaitForSeconds(0.5f);
                    newPC.UnlockInput();
                }

                yield return new WaitForSeconds(invTime);

                if (newPH != null)
                {
                    newPH.isInvincible = false;
                }
            }
        }


        // search exact-named transform in a specific scene
        private Transform FindTransformInScene(Scene scene, string exactObjectName)
        {
            if (string.IsNullOrEmpty(exactObjectName)) return null;
            if (!scene.IsValid()) return null;
            var roots = scene.GetRootGameObjects();
            foreach (var go in roots)
            {
                var found = FindInChildrenRecursive(go.transform, exactObjectName);
                if (found != null) return found;
            }
            return null;
        }

        private Transform FindInChildrenRecursive(Transform parent, string nameToFind)
        {
            if (parent.name == nameToFind) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var t = parent.GetChild(i);
                var r = FindInChildrenRecursive(t, nameToFind);
                if (r != null) return r;
            }
            return null;
        }

    }
}