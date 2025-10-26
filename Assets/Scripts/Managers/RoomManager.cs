using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using Game.Player;
using Game.UI;

namespace Game.Managers
{
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        [SerializeField] private float fadeDuration = 5f;
        [SerializeField] private float settleAfterMove = 0.05f;
        private bool isTransitioning = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }


        public void RequestRoomTeleport(string targetSceneName, string entranceObjectName, string cameraBoundsObjectName = null, bool unloadOldScene = true)
        {
            if (isTransitioning)
            {
                Debug.Log("[RoomManager] Transitioning, repetitive request ignored.");
                return;
            }

            StartCoroutine(RoomTeleportCoroutine(targetSceneName, entranceObjectName, cameraBoundsObjectName, unloadOldScene));
        }


        private IEnumerator RoomTeleportCoroutine(string targetSceneName, string entranceObjectName, string cameraBoundsObjectName, bool unloadOldScene)
        {
            isTransitioning = true;

            GameObject playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null)
            {
                Debug.LogError("[RoomManager] Player not found!");
                isTransitioning = false;
                yield break;
            }

            PlayerControl pc = playerGO.GetComponent<PlayerControl>();
            var playerHealth = playerGO.GetComponent<PlayerHealth>();
            var playerRb = playerGO.GetComponent<Rigidbody2D>();

            // fade out
            if (ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeOutCoroutine(fadeDuration);
            }
            else
            {
                yield return new WaitForSecondsRealtime(fadeDuration * 0.5f);
            }

            // lock player input and stop physics
            if (pc != null) pc.LockInput();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
                playerRb.Sleep();
            }

            // save health snapshot using reflection
            object savedHealthSnapshot = null;
            if (playerHealth != null)
            {
                try
                {
                    var t = playerHealth.GetType();
                    var field = t.GetField("currentHealth");
                    if (field != null) savedHealthSnapshot = field.GetValue(playerHealth);
                    else
                    {
                        var prop = t.GetProperty("CurrentHealth");
                        if (prop != null) savedHealthSnapshot = prop.GetValue(playerHealth);
                    }
                }
                catch { savedHealthSnapshot = null; }
            }

            Scene fromScene = playerGO.scene;

            // load target scene additively if not already loaded
            Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
            if (!targetScene.isLoaded)
            {
                var loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
                yield return new WaitUntil(() => loadOp.isDone);
                targetScene = SceneManager.GetSceneByName(targetSceneName);
                if (!targetScene.isLoaded)
                {
                    Debug.LogError($"[RoomManager] Scene: {targetSceneName} failed to load!");

                    if (pc != null) pc.UnlockInput();
                    isTransitioning = false;
                    yield break;
                }
            }

            // find entrance and teleport player
            Transform entranceTransform = FindTransformInScene(targetScene, entranceObjectName);
            if (entranceTransform == null)
            {
                Debug.LogError($"[RoomManager] '{entranceObjectName}' not found in scene '{targetSceneName}'！");

                if (pc != null) pc.UnlockInput();
                isTransitioning = false;
                yield break;
            }

            playerGO.transform.position = entranceTransform.position;

            // move player object into target scene
            if (playerGO.scene != targetScene)
            {
                SceneManager.MoveGameObjectToScene(playerGO, targetScene);
            }


            //handle camera confiner setup
            /*if (!string.IsNullOrEmpty(cameraBoundsObjectName))
            {
                var boundsTransform = FindTransformInScene(targetScene, cameraBoundsObjectName);
                if (boundsTransform != null)
                {
                    var poly = boundsTransform.GetComponent<PolygonCollider2D>();
                    if (poly != null)
                    {
                        //ApplyCameraConfiner(poly);
                        if (CameraManager.Instance != null)
                            CameraManager.Instance.ApplyCameraConfiner(poly);
                    }
                    else
                    {
                        Debug.LogWarning($"[RoomManager] PolygonCollider2D of '{cameraBoundsObjectName}' not found in '{targetSceneName}'");
                    }
                }
                else
                {
                    Debug.LogWarning($"[RoomManager] camera bounds '{cameraBoundsObjectName}' not found in '{targetSceneName}'");
                }
            }
            else
            {
                // fallbakc: auto find the first polygon collider 2d in scene
                var autoPoly = FindFirstPolygonColliderInScene(targetScene);
                if (autoPoly != null && CameraManager.Instance != null) CameraManager.Instance.ApplyCameraConfiner(autoPoly);
            }*/



            //call SetupCameraConfiner
            yield return StartCoroutine(BindCameraConfinerWhenReady(targetScene, cameraBoundsObjectName));

            // wait a bit for camera and movement to settle
            yield return new WaitForSecondsRealtime(settleAfterMove);

            //let camera follow current player
            CameraManager.Instance.FollowPlayer(playerGO);

            // unload previous scene
            if (unloadOldScene && fromScene.isLoaded)
            {
                if (fromScene.name != this.gameObject.scene.name)
                {
                    var unloadOp = SceneManager.UnloadSceneAsync(fromScene);
                    if (unloadOp != null)
                        yield return new WaitUntil(() => unloadOp.isDone);
                }
            }

            // fade in
            if (ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeInCoroutine(fadeDuration);
            }
            else
            {
                yield return new WaitForSecondsRealtime(fadeDuration * 0.5f);
            }

            // unlock player input
            if (pc != null) pc.UnlockInput();

            //restore saved health value
            if (playerHealth != null && savedHealthSnapshot != null)
            {
                try
                {
                    var t = playerHealth.GetType();
                    var field = t.GetField("currentHealth");
                    if (field != null) field.SetValue(playerHealth, savedHealthSnapshot);
                }
                catch { }
            }

            isTransitioning = false;
        }

        // Recursive search for an object by exact name in the scene hierarchy
        private Transform FindTransformInScene(Scene scene, string exactObjectName)
        {
            if (!scene.IsValid()) return null;
            var roots = scene.GetRootGameObjects();
            foreach (var go in roots)
            {
                var found = FindInChildrenRecursive(go.transform, exactObjectName);
                if (found != null) return found;
            }
            return null;
        }

        // Recursive search helper
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

        // finds first polygon collider 2d in the scene to use as default bounds
        private PolygonCollider2D FindFirstPolygonColliderInScene(Scene scene)
        {
            if (!scene.IsValid()) return null;
            var roots = scene.GetRootGameObjects();
            foreach (var go in roots)
            {
                var poly = go.GetComponentInChildren<PolygonCollider2D>();
                if (poly != null) return poly;
            }
            return null;
        }

        //handle camera confiner setup
        private void SetupCameraConfiner(Scene targetScene, string cameraBoundsObjectName = null)
        {
            if (CameraManager.Instance == null) return;

            PolygonCollider2D poly = null;

            if (!string.IsNullOrEmpty(cameraBoundsObjectName))
            {
                Transform boundsTransform = FindTransformInScene(targetScene, cameraBoundsObjectName);
                if (boundsTransform != null)
                    poly = boundsTransform.GetComponent<PolygonCollider2D>();
            }

            // find object name "CameraBound" in current scene
            if (poly == null)
            {
                Transform boundsTransform = FindTransformInScene(targetScene, "CameraBound");
                if (boundsTransform != null)
                    poly = boundsTransform.GetComponent<PolygonCollider2D>();
            }

            // find out first PolygonCollider2D in current scene
            if (poly == null)
                poly = FindFirstPolygonColliderInScene(targetScene);

            if (poly != null) {

                Debug.Log($"[RoomManager] CameraConfiner set to: {poly.gameObject.name}");
                CameraManager.Instance.ApplyCameraConfiner(poly);
            }
                
            else
                Debug.LogWarning($"[RoomManager] No PolygonCollider2D found in scene '{targetScene.name}' for camera confiner");
        }

        // Before calling SetupCameraConfiner,make sure the scene is loaded
        public IEnumerator BindCameraConfinerWhenReady(Scene targetScene, string cameraBoundsObjectName = null)
        {
            
            yield return new WaitUntil(() => targetScene.isLoaded && targetScene.rootCount > 0);

            SetupCameraConfiner(targetScene, cameraBoundsObjectName);
        }
    }
}
