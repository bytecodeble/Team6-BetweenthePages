using UnityEngine;

namespace Game.Managers
{

    public class CameraControl : MonoBehaviour
    {
        // Transform used to track players
        private Transform player;
        private GameObject playerObject;
        [SerializeField] private Collider2D cameraBounds;

        // Limit the boundary of the camera's movement range (put a BoxCollider2D in the scene)
        private float halfHeight;
        private float halfWidth;

        void Start()
        {
            //half of camera hight we can see
            halfHeight = Camera.main.orthographicSize;
            //aspect = width/hight 
            halfWidth = halfHeight * Camera.main.aspect;
        }

        // LateUpdate is called once per frame
        // Player Update has updated its position
        //Move the camera again and the picture will be smoother
        void LateUpdate()
        {
            //find the player in scene, use Tag
            if (player == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    //save Transform of player for player.position
                    player = playerObject.transform;
                }
                else
                {
                    return; //if player is not in scene, camera is not move
                }
            }

            float clampedX = Mathf.Clamp(
                player.position.x,
                cameraBounds.bounds.min.x + halfWidth,
                cameraBounds.bounds.max.x - halfWidth
            );

            float clampedY = Mathf.Clamp(
                player.position.y,
                cameraBounds.bounds.min.y + halfHeight,
                cameraBounds.bounds.max.y - halfHeight
            );

            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }
    }
}
