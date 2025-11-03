using Game.Player;
using Game.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class EndingEntrance : MonoBehaviour
    {
        public string endingSceneName = "EndingScene";
        public bool destroyPlayer = true;
        public float fadeDuration = 1.0f;

        private bool triggered = false;

        private void Reset()
        {
            var c = GetComponent<Collider2D>();
            c.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered) return;
            if (!other.CompareTag("Player")) return;

            triggered = true; //prevent multiple trigger requests

            StartCoroutine(HandleEndingSequence(other.gameObject));
        }

        private IEnumerator HandleEndingSequence(GameObject playerGO)
        {
            // lock player input and stop physics
            var pc = playerGO.GetComponent<PlayerControl>();
            if (pc != null) pc.LockInput();

            var rb = playerGO.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.Sleep();
            }

            // fade out
            if (ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeOutCoroutine(fadeDuration);
            }
            else
            {
                yield return new WaitForSecondsRealtime(fadeDuration);
            }

            if (destroyPlayer)
            {
                Destroy(playerGO);
            }
            else
            {
                playerGO.SetActive(false);
            }

            // load ending scene in single mode
            var loadOp = SceneManager.LoadSceneAsync(endingSceneName, LoadSceneMode.Single);
            if (loadOp == null)
            {
                Debug.LogError($"[EndingEntrance] Failed start loading scene '{endingSceneName}'");
                yield break;
            }
            yield return new WaitUntil(() => loadOp.isDone);

            // UI prepare time
            yield return new WaitForSecondsRealtime(0.05f);

            // fade in
            if (ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeInCoroutine(fadeDuration);
            }
            else
            {
                yield return new WaitForSecondsRealtime(fadeDuration * 0.5f);
            }
        }
    }
}
