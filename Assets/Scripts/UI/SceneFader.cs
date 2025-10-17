using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance;

        [SerializeField] private Image overlay;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (overlay != null)
            {
                // start transparent
                Color c = overlay.color;
                c.a = 0f;
                overlay.color = c;
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
    }
}

