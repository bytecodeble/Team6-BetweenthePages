using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]private GameObject playerPrefab;
    [SerializeField]private Transform respawnPoint;

    private GameObject currentPlayer;
    void Start()
    {
        RespawnPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RespawnPlayer()
    {
        //Make sure the player instance on the field has been destroyed.
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        // Spawn a new player Prefab at the respawn point
        currentPlayer = Instantiate(playerPrefab, respawnPoint.position, Quaternion.identity);
    }
}
