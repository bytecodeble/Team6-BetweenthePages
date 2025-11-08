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
            SetAlpha(0f);
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

            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (loadOp == null)
            {
                Debug.LogError($"[ScreenFader] Failed start loading scene '{sceneName}'");
                yield break;
            }

            yield return new WaitUntil(() => loadOp.isDone);
            yield return new WaitForSecondsRealtime(0.05f);
            yield return FadeInCoroutine(duration);
        }
    }
}

