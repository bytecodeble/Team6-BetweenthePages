using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button returnToMenuButton;
    public Slider musicSlider;

    [Header("Scene")]
    public string menuSceneName = "MenuScene";

    private bool isPaused = false;

    void Start()
    {
        // 初始隐藏面板
        pausePanel.SetActive(false);

        // 确保按钮引用已经拖好
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (returnToMenuButton != null) returnToMenuButton.onClick.AddListener(ReturnToMenu);

        // 初始化 Slider
        if (musicSlider != null)
        {
            if (MusicManager.instance != null)
                musicSlider.value = MusicManager.instance.GetVolume();
            else
                musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

            musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        }
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
