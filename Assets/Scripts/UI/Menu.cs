using Game.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI
{
    public class Menu : MonoBehaviour
    {
        public IntroPanelController introPanel;
        void Start()
        {
            Time.timeScale = 1.0f;
        }

        public void Play()
        {
            /*
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }
            else
            {
                Debug.LogError("[MenuController] GameManager instance not found!");
            }*/
            introPanel.ShowIntro();
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene("MenuScene");
            Time.timeScale = 1.0f;
        }

        public void PauseGame()
        {
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
        }

        public void QuitGame()
        {
            Application.Quit();
        }

    }
}