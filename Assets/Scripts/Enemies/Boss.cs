using Game.Environment;
using Game.Managers;
using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Boss : BaseEnemy
    {
        //Boss setting
        private float chaseSpeed = 4f;
        public float meleeRange = 4f;
        public float meleeVerticalRange = 2f;

        public float restMin = 0.7f;
        public float restMax = 1.5f;

        // jump attack settings
        public float jumpChance = 0.3f;
        public float jumpDuration = 1.5f;
        public float jumpPause = 0.5f;
        public float jumpApexHeight = 4f;

        // dash attack settings
        public float dashChance = 0.5f;
        public float dashWindupTime = 0.4f;
        public float dashAttackDuration = 0.2f;
        public float dashStunDuration = 0.4f;

        // limit combo attacks prevent soft lock
        public int maxComboAttacks = 2;
        public int comboAttackCount = 0;
        public Vector3 roomCenterPos = new Vector3(-30, -3, 0);

        private Collider2D col;

        public GameObject attackHitboxPrefab;
        public GameObject chargeEffectPrefab;
        [SerializeField] private GameObject redCloak;

        // Spine Animation
        [Header("Animation")]
        [SerializeField] private SkeletonAnimation spineAnimation;
        private const int TRACK_INDEX = 0;

        private const string ANIM_IDLE = "idle";
        private const string ANIM_ATTACK = "attack";
        private const string ANIM_JUMP = "jump";
        private const string ANIM_DEATH = "death";
        private const string ANIM_CHASE = "chase";

        // Hurt flash settings
        private Color hurtColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
        private Coroutine hurtFlashRoutine;
        private Color originalColor = Color.white;

        //Debug gizmos
        private Color detectionColor = Color.yellow;
        private Color meleeColor = Color.red;
        private float gizmoYOffset = 2.55f;

        //soulorb 
        [Header("SoulOrb VFX")]
        [SerializeField] private GameObject soulOrbPrefab;

        [SerializeField] private GameObject damageEffect;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 30;
            currentHealth = maxHealth;

            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();

            detectionRange = 10f;

            if (spineAnimation == null)
                spineAnimation = GetComponent<SkeletonAnimation>();

            if (spineAnimation != null && spineAnimation.AnimationState != null && spineAnimation.AnimationState.Data != null)
            {
                var data = spineAnimation.AnimationState.Data;
                data.SetMix(ANIM_ATTACK, ANIM_IDLE, 0.15f);
                data.SetMix(ANIM_JUMP, ANIM_IDLE, 0.15f);
                data.SetMix(ANIM_CHASE, ANIM_IDLE, 0.15f);
            }

            // cache original color from skeleton (RGB only; A controlled by attachments)
            if (spineAnimation != null && spineAnimation.Skeleton != null)
            {
                originalColor = new Color(spineAnimation.Skeleton.R, spineAnimation.Skeleton.G, spineAnimation.Skeleton.B, 1f);
            }

            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
            }
            else
            {
                Debug.LogError("FATAL: Boss cannot find player object with tag 'Player' in scene!");
            }
        }

        public override EnemyState GetInitialState()
        {
            return new BossPatrolState(this);
        }

        public override void ApplyKnockback(Vector2 hitSource, float force = 5f, float duration = 0.2f)
        {
            // Boss immune to knockback
        }

        public override void TakeDamage(int damage)
        {
            if (IsDead) return;

            currentHealth -= damage;

            //spawn blood splash for boss
            if (damageEffect != null)
            {
                GameObject effect = Instantiate(damageEffect, transform.position + Vector3.up * 1.7f, Quaternion.identity);
                Destroy(effect, 1f);
            }

            if (currentHealth > 0)
            {
                StartHurtFlash();
            }

            Debug.Log($"Boss.TakeDamage: -{damage} HP, current = {currentHealth}");

            if (currentHealth <= 0) Die();
        }

        protected override void Die()
        {
            if (IsDead) return;
            IsDead = true;
            currentState = null;

            DropSoulOrb(100);

            StopAllCoroutines();
            // ensure color reset before death animation
            ResetSkeletonColor();

            Debug.Log("Boss.Die: Boss defeated.");

            if (col != null)
                col.enabled = false;
            if (rb != null)
            {
                rb.simulated = false;
                rb.linearVelocity = Vector2.zero;
            }

            DeathAnimation(() =>
            {
                Vector3 cloakSpawnPos = new Vector3(-30, -0.5f, 0);
                if (redCloak != null)
                {
                    Instantiate(redCloak, cloakSpawnPos, Quaternion.identity);
                }

                Destroy(gameObject);
            });
        }

        private void StartHurtFlash()
        {
            if (spineAnimation == null || spineAnimation.Skeleton == null) return;

            if (hurtFlashRoutine != null)
                StopCoroutine(hurtFlashRoutine);
            hurtFlashRoutine = StartCoroutine(HurtFlashCoroutine(0.5f));
        }

        private IEnumerator HurtFlashCoroutine(float duration)
        {
            var skeleton = spineAnimation.Skeleton;
            float timer = 0f;
            float interval = 0.1f;
            bool showRed = true;
            while (timer < duration && !IsDead)
            {
                if (showRed)
                {
                    skeleton.R = hurtColor.r;
                    skeleton.G = hurtColor.g;
                    skeleton.B = hurtColor.b;
                    skeleton.A = hurtColor.a;
                }
                else
                {
                    skeleton.R = originalColor.r;
                    skeleton.G = originalColor.g;
                    skeleton.B = originalColor.b;
                    skeleton.A = 0.7f;
                }
                showRed = !showRed;
                float step = Mathf.Min(interval, duration - timer);
                timer += step;
                yield return new WaitForSeconds(step);
            }
            ResetSkeletonColor();
            skeleton.A = 1f;
            hurtFlashRoutine = null;
        }

        private void ResetSkeletonColor()
        {
            if (spineAnimation == null || spineAnimation.Skeleton == null) return;
            var skeleton = spineAnimation.Skeleton;
            skeleton.R = originalColor.r;
            skeleton.G = originalColor.g;
            skeleton.B = originalColor.b;
        }

        public bool IsPlayerInRangeFloat(float range)
        {
            if (IsDead) return false;

            if (player == null)
            {
                Debug.LogError("FATAL ERROR: Boss's player reference is NULL! Cannot calculate distance.");
                return false;
            }
            float dist = Vector2.Distance(transform.position, player.position);
            Debug.Log($"Distance Check: Boss Pos={transform.position}, Player Pos={player.position}, Calculated Dist={dist:F2}, Range={range}");
            return dist <= range;
        }

        public void MoveTowardsPlayerX()
        {
            if (IsDead || player == null) return;
            float dir = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
            FacePlayer();
        }

        public void StopMovement()
        {
            if (IsDead) return;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // Animation helpers
        public void IdleAnimation()
        {
            if (spineAnimation == null) return;
            if (spineAnimation.AnimationName != ANIM_IDLE)
                spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_IDLE, true);
        }

        public void AttackAnimation()
        {
            if (spineAnimation == null) return;
            spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_ATTACK, false);
        }

        public void JumpAnimation()
        {
            if (spineAnimation == null) return;
            spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMP, true);
        }

        public void ChaseAnimation()
        {
            if (spineAnimation == null) return;
            if (spineAnimation.AnimationName != ANIM_CHASE)
                spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_CHASE, true);
        }

        public void DeathAnimation(System.Action onComplete = null)
        {
            if (spineAnimation == null)
            {
                onComplete?.Invoke();
                return;
            }

            var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_DEATH, false);
            if (onComplete != null)
            {
                entry.Complete += (e) => onComplete();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 gizmoCenter = transform.position + Vector3.up * gizmoYOffset;

            Gizmos.color = detectionColor;
            Gizmos.DrawWireSphere(gizmoCenter, detectionRange);
            
            Gizmos.color = meleeColor;
            Gizmos.DrawWireSphere(gizmoCenter, meleeRange);
        }

        //drop a SoulOrb when enemy die
        private void DropSoulOrb(int score)
        {
            if (soulOrbPrefab != null)
            {
                GameObject orb = Instantiate(soulOrbPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
                orb.GetComponent<SoulOrb>().SetValue(score);
            }
        }
    }
}
