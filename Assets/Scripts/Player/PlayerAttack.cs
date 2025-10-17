using System.Collections;
using UnityEngine;

namespace Game.Player
{

    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private GameObject attackHitbox;
        [SerializeField] private float attackDuration = 0.4f;
        private Vector2 hitboxOffset = new Vector2(1f, 0.5f);

        private GameObject activeHitbox;
        private Transform playerTransform;


        public delegate void AttackEvent();
        public event AttackEvent OnAttackPerformed;

        private bool isAttacking = false;


        void Awake()
        {
            playerTransform = transform;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.J) && !isAttacking)
            {
                StartCoroutine(PerformAttack());
            }
        }

        private IEnumerator PerformAttack()
        {
            isAttacking = true;
            OnAttackPerformed?.Invoke();

            activeHitbox = Instantiate(attackHitbox, playerTransform);

            float elapsed = 0f;
            while (elapsed < attackDuration)
            {
                if (activeHitbox != null)
                    activeHitbox.transform.localPosition = hitboxOffset;


                elapsed += Time.deltaTime;
                yield return null;
            }

            if (activeHitbox != null)
                Destroy(activeHitbox);

            isAttacking = false;

        }
    }

}