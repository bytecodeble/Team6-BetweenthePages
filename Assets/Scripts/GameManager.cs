using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton mode (ensuring there is only one GameManager globally)
    public static GameManager Instance;

    //generate new player instances(player prefab)
    [SerializeField]private GameObject playerPrefab;
    //The position where the player spawns (an empty object)
    [SerializeField]private Transform respawnPoint;

    //Current player instance
    private GameObject currentPlayer;

    // Nullable: last saved position by SaveManager
    private Vector3? savedPosition = null;
    void Start()
    {
        RespawnPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void Awake()
    {
        //Make sure there is only one GameManager in the scene and it is not destroyed when switching scenes
        if (Instance == null)
        {
            //Keep this object for the next scene
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            //If there is already an instance, the extra one is destroyed.
            Destroy(gameObject);
        }
    }

    public void OnPlayerDeath()
    {
        //uese this function When player died
        RespawnPlayer();
    }

    //get position values from SaveManager
    public void SetSavePoint(Vector3 position)
    {
        savedPosition = position;
    }

    public void RespawnPlayer()
    {
        //Make sure the player instance on the field has been destroyed.
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        Vector3 spawnPos;

        //if player click the save point
        if(savedPosition != null)
        {
            spawnPos = (Vector3)savedPosition;
        }
        else
        {
            spawnPos = respawnPoint.position;
        }
            
        // Spawn a new player Prefab at the respawn point
        currentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

   
}
