using UnityEngine;
using Game.Managers;
using Game.Player;

namespace Game.Environment
{
    public class RedCloakItem : MonoBehaviour
    {
        private bool collected = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collected) return;

            if (collision.CompareTag("Player"))
            {
                collected = true;
                var player = collision.GetComponent<PlayerControl>();
                if (player != null)
                {
                    AbilityManager.Instance.StartDoubleJumpAcquisition(player, this);
                }
            }
        }
    }
}
