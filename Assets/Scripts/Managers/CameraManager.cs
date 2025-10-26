using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    [SerializeField]private CinemachineCamera CineCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    //Cinecamera follow player in new scene
    public void FollowPlayer(GameObject player)
    {
        if (CineCamera != null && player != null)
        {
            CineCamera.Target.TrackingTarget = player.transform;
        }
    }

    //apply new bounds to cinemachine confiner2d
    public void ApplyCameraConfiner(PolygonCollider2D boundsPoly)
    {
        var confiner = CineCamera.GetComponent<CinemachineConfiner2D>();
        if (confiner != null)
        {
            confiner.BoundingShape2D = boundsPoly;
#if UNITY_2021_2_OR_NEWER
            confiner.InvalidateBoundingShapeCache();
#endif
        }
    }
}
