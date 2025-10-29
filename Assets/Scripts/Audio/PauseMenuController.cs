using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button returnToMenuButton;
    public Slider musicSlider;
    public string menuSceneName = "MenuScene";

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);

        pauseButton.onClick.AddListener(TogglePause);
        resumeButton.onClick.AddListener(Resume);
        returnToMenuButton.onClick.AddListener(ReturnToMenu);

        if (MusicManager.instance != null)
        {
            musicSlider.value = MusicManager.instance.GetVolume();
        }
        else
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        }

        musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    void ChangeMusicVolume(float value)
    {
        if (MusicManager.instance != null)
            MusicManager.instance.SetVolume(value);
        else
            PlayerPrefs.SetFloat("MusicVolume", value);
    }
}
