using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Game.Managers
{
    public class EventsystemManager : MonoBehaviour
    {
        private static EventsystemManager instance;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Debug.LogWarning("EventSystem: More than one EventSystem detected. ");
                Destroy(gameObject);
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

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EventSystem[] allEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

            if (allEventSystems.Length > 1)
            {
                for (int i = 0; i < allEventSystems.Length; i++)
                {
                    if (allEventSystems[i].gameObject != this.gameObject)
                    {
                        Destroy(allEventSystems[i].gameObject);
                    }
                }
            }
        }
    }
}