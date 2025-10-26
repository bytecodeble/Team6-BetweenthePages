using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    
    public string gameSceneName = "TutorialRoom"; 

    
    public void OnPlayPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    
    public void ToggleOptions(GameObject optionsPanel)
    {
        bool isActive = optionsPanel.activeSelf;
        optionsPanel.SetActive(!isActive);
    }
}
