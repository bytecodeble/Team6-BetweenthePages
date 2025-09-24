using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private PlayerControl player;
    public BoxCollider2D cameraBounds;

    private float halfHeight;
    private float halfWidth;
 
    void Start()
    {
        player = FindAnyObjectByType<PlayerControl>();

        halfHeight = Camera.main.orthographicSize;  //half of camera hight we can see
        halfWidth = halfHeight * Camera.main.aspect; //aspect = width/hight ,aspect is w
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null) 
        {
            transform.position = new Vector3(
                Mathf.Clamp(player.transform.position.x,cameraBounds.bounds.min.x + halfWidth, cameraBounds.bounds.max.x - halfWidth),
                Mathf.Clamp(player.transform.position.y, cameraBounds.bounds.min.y + halfHeight, cameraBounds.bounds.max.y - halfHeight),
                transform.position.z);
        }
    }
}
