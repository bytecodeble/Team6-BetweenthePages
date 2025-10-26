using Game.Managers;
using UnityEngine;

public class MenuController : MonoBehaviour
{


    
    public void OnPlayPressed()
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

    
    public void ToggleOptions(GameObject optionsPanel)
    {
        bool isActive = optionsPanel.activeSelf;
        optionsPanel.SetActive(!isActive);
    }
}
