using UnityEngine;
using UnityEngine.UI;
using Game.Managers;  
using UnityEngine.SceneManagement;

namespace Game.UI
{
    public class IntroPanelController : MonoBehaviour
    {
        public GameObject panel;
        public Image displayImage;
        public Sprite[] introImages;
        public Button nextButton;
        public Button startButton;

        private int index = 0;

        void Start()
        {
            panel.SetActive(false);
            startButton.gameObject.SetActive(false);
        }

        public void ShowIntro()
        {
            index = 0;
            panel.SetActive(true);
            startButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
            displayImage.sprite = introImages[index];
        }

        public void NextImage()
        {
            index++;

            if (index < introImages.Length)
            {
                displayImage.sprite = introImages[index];
            }

            // the last one openPage will show the start button
            if (index == introImages.Length - 1)
            {
                nextButton.gameObject.SetActive(false);
                startButton.gameObject.SetActive(true);
            }
        }

        public void StartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }
            else
            {
                Debug.LogError("[MenuController] GameManager instance not found!");
            }

        }
    }
}
