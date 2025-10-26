using UnityEngine;

namespace Game.Environment
{
    public class RoomEntrance : MonoBehaviour
    {
        public string targetScene;
        public string targetEntrance;
        public string targetCameraBounds;
        public bool unloadCurrentScene = true;


        private void Reset()
        {
            var c = GetComponent<Collider2D>();
            c.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            if (string.IsNullOrEmpty(targetScene) || string.IsNullOrEmpty(targetEntrance))
            {
                Debug.LogWarning($"[RoomEntrance] targetScene or targetEntrance not set on {gameObject.name}");
                return;
            }

            if (Game.Managers.RoomManager.Instance != null)
            {
                Game.Managers.RoomManager.Instance.RequestRoomTeleport(
                    targetScene,
                    targetEntrance,
                    targetCameraBounds,
                    unloadCurrentScene
                );
            }
            else
            {
                Debug.LogWarning("[RoomEntrance] RoomManager not found!");
            }
        }
    }
}
