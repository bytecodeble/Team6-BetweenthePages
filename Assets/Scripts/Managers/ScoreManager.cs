using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Game.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance;

        [SerializeField] private GameObject scoreCanvas;
        [SerializeField] private TMP_Text scoreText;
        private int score = 0;

        [SerializeField] private string[] showScoreScenes;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                if (scoreCanvas != null)
                    DontDestroyOnLoad(scoreCanvas);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        //only 3 Rooms can show the score UI
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool show = false;
            foreach (var sc in showScoreScenes)
            {
                if (scene.name == sc) { show = true; break; }
            }

            if (scoreCanvas != null)
                scoreCanvas.SetActive(show); 

            UpdateUI();
        }

        public void AddScore(int amount)
        {
            score += amount;
            UpdateUI();
        }


        private void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = score.ToString();
        }
       

    }
}
