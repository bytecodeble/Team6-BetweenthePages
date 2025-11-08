using System.Collections;
using UnityEngine;

namespace Game.Player
{

    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private GameObject attackHitbox;
        [SerializeField] private GameObject attackEffect;
        private float hitboxWindow = 0.10f;
        private float hardstun = 0.3f;
        private float attackLockDuration = 0.41f;
        private float recovery = 0.16f;
        private float inputBufferWindow = 0.12f;
        private Vector2 hitboxOffset = new Vector2(1f, 0.5f);
        private Vector3 effectOffset = new Vector3(1.5f, 0.8f, 0f);


        private GameObject activeHitbox;
        private GameObject activeEffect;
        private Transform playerTransform;

        public delegate void AttackEvent();
        public event AttackEvent OnAttackPerformed;

        private bool isAttacking = false;
        private bool queuedAttack = false;
        private float attackStartTime = 0f;


        void Awake()
        {
            playerTransform = transform;

            // clamp sensible values
            hitboxWindow = Mathf.Max(0f, hitboxWindow);
            hardstun = Mathf.Max(0f, hardstun);
            recovery = Mathf.Max(0f, recovery);

            attackLockDuration = hitboxWindow + hardstun + recovery;
            inputBufferWindow = Mathf.Clamp(inputBufferWindow, 0f, attackLockDuration);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                if (!isAttacking)
                {
                    StartCoroutine(PerformAttack());
                }
                else
                {
                    // if already attacking, allow buffer or queue only within the inputBuffer near the end
                    float t = Time.time - attackStartTime;
                    float timeUntilLockEnds = attackLockDuration - t;
                    if (timeUntilLockEnds <= inputBufferWindow && timeUntilLockEnds > 0f)
                    {
                        queuedAttack = true;
                    }
                }
            }
        }

        private IEnumerator PerformAttack()
        {
            isAttacking = true;

            while (true)
            {
                queuedAttack = false;
                attackStartTime = Time.time;
                OnAttackPerformed?.Invoke();

                //spawn attact effect
                if(attackEffect != null)
                {
                    activeEffect = Instantiate(attackEffect, playerTransform);
                    activeEffect.transform.localPosition = effectOffset;

                    //effect flip
                    Vector3 effectEuler = activeEffect.transform.localEulerAngles;
                    effectEuler.y = (playerTransform.localScale.x < 0f) ? 180f : 0f;
                    activeEffect.transform.localEulerAngles = effectEuler;

                    //play on awake effect
                    ParticleSystem ps = activeEffect.GetComponent<ParticleSystem>();
                    if (ps != null && !ps.isPlaying)
                    {
                        ps.Play();
                        Destroy(activeEffect, ps.main.duration + ps.main.startLifetime.constantMax);
                    }
                    else
                    {
                       
                        Destroy(activeEffect, 1.1f);
                    }
                }

                //spawn hitbox
                if (attackHitbox != null)
                {
                    activeHitbox = Instantiate(attackHitbox, playerTransform);
                }

                //active hitbox window
                float elapsed = 0f;
                while (elapsed < hitboxWindow)
                {
                    if (activeHitbox != null)
                        activeHitbox.transform.localPosition = hitboxOffset;

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                //destroy hitbox after damage window
                if (activeHitbox != null)
                {
                    Destroy(activeHitbox);
                    activeHitbox = null;
                }

                float hardElapsed = 0f;
                while (hardElapsed < hardstun)
                {
                    hardElapsed += Time.deltaTime;
                    yield return null;
                }

                float recoveryElapsed = 0f;
                float remianingRecovery = recovery;
                while (recoveryElapsed < remianingRecovery)
                {
                    if (queuedAttack) break;
                    
                    recoveryElapsed += Time.deltaTime;
                    yield return null;
                }

                if (queuedAttack)
                {
                    yield return null;
                    continue;
                }
                else
                {
                    break;
                }

            }
            isAttacking = false;
            queuedAttack = false;
        }
    }

}