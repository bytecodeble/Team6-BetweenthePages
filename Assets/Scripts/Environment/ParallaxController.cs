using UnityEngine;

namespace Game.Environment
{
    public class ParallaxController : MonoBehaviour
    {
        //Main camera
        [SerializeField] private GameObject cam;

        //foreground is 0,middleground is 1,the bigger the number, the farther
        [SerializeField] private int layerIndex;
        [SerializeField] private int totalLayers = 4;

        [SerializeField, Range(0f, 1f)] private float globalScale = 0.5f;

        private float parallaxEffect;
        private float startPos;

        void Start()
        {
            //The initial x-axis coordinate of the sprite center point
            startPos = transform.position.x;

            //The foreground is faster, the background is slower
            float foregroundSpeed = 0.3f;
            float backgroundSpeed = 0.05f;
            float t = (float)layerIndex / (totalLayers - 1f); // 0~1
            parallaxEffect = Mathf.Lerp(foregroundSpeed, backgroundSpeed, t);
        }

        void LateUpdate()
        {
        
            float distance = cam.transform.position.x * parallaxEffect * globalScale;

            //update new position of backgrounds
            transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);
        }
    }
}
