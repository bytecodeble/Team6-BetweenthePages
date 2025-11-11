using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Game.Player;

namespace Game.UI
{
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance;
        private bool isTransitioning = false;
        [SerializeField] private Image overlay;

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
                return;
            }

            // if not binding with overlay then attemp search
            if (overlay == null)
            {
                overlay = GetComponentInChildren<Image>();
            }
            // search failed
            if (overlay == null)
            {
                Debug.LogError("[ScreenFader] No Image found for overlay!");
                return;
            }

            if (overlay != null)
            {
                // start transparent
                Color c = overlay.color;
                c.a = 0f;
                overlay.color = c;
            }

            //make sure RectTransform in overlay fullscreen
            if (overlay != null)
            {
                RectTransform rt = overlay.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (overlay == null)
            {
                overlay = GetComponentInChildren<Image>(true);
                if (overlay == null)
                {
                    Debug.LogWarning($"[ScreenFader] No overlay image found after loading {scene.name}!");
                    return;
                }
            }
        }

        public IEnumerator FadeOutCoroutine(float duration)
        {
            if (overlay == null) yield break;

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(t / duration);
                SetAlpha(alpha);
                yield return null;
            }
            SetAlpha(1f);
        }

        public IEnumerator FadeInCoroutine(float duration)
        {
            if (overlay == null) yield break;

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(1f - (t / duration));
                SetAlpha(alpha);
                yield return null;
            }
            SetAlpha(0f);
        }

        private void SetAlpha(float a)
        {
            if (overlay == null) return;
            Color c = overlay.color;
            c.a = a;
            overlay.color = c;
        }

        public IEnumerator FadeToSceneCoroutine(GameObject playerGO, string sceneName, float duration, bool destroyPlayer)
        {
            isTransitioning = true;
            Scene currentScene = SceneManager.GetActiveScene();

            // lock player input and stop physics
            if (playerGO != null)
            {
                var pc = playerGO.GetComponent<PlayerControl>();
                if (pc != null) pc.LockInput();

                var rb = playerGO.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.Sleep();
                }
            }

            yield return FadeOutCoroutine(duration);

            if (playerGO != null)
            {
                if (destroyPlayer)
                {
                    Destroy(playerGO);
                }
                else
                {
                    playerGO.SetActive(false);
                }
            }

            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive); 
            if (loadOp == null)
            {
                Debug.LogError($"[ScreenFader] Failed start loading scene '{sceneName}'");
                isTransitioning = false;
                yield break;
            }

            yield return new WaitUntil(() => loadOp.isDone);

            Scene newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.IsValid() && newScene.isLoaded)
            {
                SceneManager.SetActiveScene(newScene);
            }
            if (currentScene.isLoaded && currentScene.name != sceneName)
            {
                yield return SceneManager.UnloadSceneAsync(currentScene);
            }

            yield return new WaitForSecondsRealtime(0.05f);
            yield return FadeInCoroutine(duration);

            isTransitioning = false;
        }
    }
}

