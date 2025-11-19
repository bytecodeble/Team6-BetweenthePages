using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Player
{
    public class PlayerControl : MonoBehaviour
    {
        private Rigidbody2D RB;
        private BoxCollider2D Collider;
        private PlayerHealth playerHealth;
        private PlayerAttack playerAttack;

        #region Movement Variables
        [Header("Walk")]
        private float maxHorizontalSpeed = 7.5f;
        private float horizontalAcceleration = 140;
        private float groundFriction = 150;
        private float airFriction = 20;

        [Header("Jump")]
        private float jumpPower = 19.18f;//
        private float maxFallSpeed = 42f;
        private float riseGravity = 46f;//
        private float jumpCutMultiplier = 1.9f; // gravity multiplier added when jump is released early
        private float fallingMultiplier = 1.1f; // make falling faster
        private float coyoteTime = 0.1f;
        private float jumpBufferTime = 0.07f;

        [Header("Double Jump")]
        private float doubleJumpPower = 17.28f;//
        public int maxDoubleJump = 0;

        private float accelerationRate;
        private bool isJumping;
        private int doubleJumpRemaining;
        public bool hasDoubleJump = false;
        #endregion

        #region Stats
        [Header("Ground Detection")]
        [SerializeField] private Transform groundPoint;
        [SerializeField] private LayerMask whatIsGround;
        private bool isOnGround;

        [Header("Inner Stats")]
        private float horizontalInput;
        private Vector2 frameVelocity;

        [Header("Timer")]
        private float coyoteTimer;
        private float jumpBufferTimer;

        [Header("Knockback")]
        private float knockbackHorizontal = 12f;
        private float knockbackVertical = 8f;
        private float knockbackDuration = 0.25f;
        private float knockbackHorizontalDecay = 20f;
        private bool isKnockback = false;

        #endregion

        #region Animation

        [Header("Animation")]
        [SerializeField] private SkeletonAnimation spineAnimation;

        private const int TRACK_INDEX = 0;

        private bool inputLocked = false;
        private bool animationLocked = false;

        private const string ANIM_IDLE = "idle";
        private const string ANIM_RUN = "run";
        private const string ANIM_JUMPSTART = "jump_start";
        private const string ANIM_JUMPRISE = "jump_rise";
        private const string ANIM_JUMPFALL = "jump_fall";
        private const string ANIM_JUMPLAND = "jump_land";
        //private const string ANIM_DOUBLEJUMPRISE = "double_jump";
        private const string ANIM_ATTACK = "attack";
        private const string ANIM_HURT = "hurt";
        private const string ANIM_DEATH = "death";

        // flickers
        private Coroutine flickerRoutine;
        private Color originalColor;
        private MeshRenderer spineRenderer;


        // animation debugger
        private string lastAnimName = "";
        //private bool playingJumpStart = false;
        //private bool playingDoubleJump = false;
        private bool playingJumpLand = false;

        // death state (mirror Boss.cs style)
        private bool isDead = false;
        public bool IsDead => isDead;

        #endregion

        #region Jump Debugging

        [Header("Debug Jump Stats")]
        [SerializeField] private bool enableJumpLogging = true; // toggle from Inspector
        private bool isTrackingAirborne = false;
        private float airborneStartTime;
        private Vector2 airborneStartPos;
        private float airbornePeakY;
        private float airborneMaxHorizontalDelta;

        // collected records for further analysis
        private struct JumpRecord
        {
            public float startTime;
            public float airtime;
            public float peakHeight; // units above start Y
            public float horizontalDisplacementLanding; // landingX - startX
            public float maxHorizontalDelta; // maximum abs(x - startX) reached during airborne
            public Vector2 startPos;
            public Vector2 landingPos;
        }
        private List<JumpRecord> jumpRecords = new List<JumpRecord>();

        #endregion



        private void Awake()
        {
            Debug.Log($"[Awake] hasDoubleJump={hasDoubleJump}, maxDoubleJump={maxDoubleJump}");

            RB = GetComponent<Rigidbody2D>();
            Collider = GetComponent<BoxCollider2D>();
            playerHealth = GetComponent<PlayerHealth>();
            playerAttack = GetComponent<PlayerAttack>();
            spineRenderer = spineAnimation.GetComponent<MeshRenderer>();

            RB.gravityScale = 0; // We use our own calculations instead of the default gravity

            doubleJumpRemaining = maxDoubleJump;

            if (spineAnimation == null)
                spineAnimation = GetComponent<SkeletonAnimation>();

            if (spineAnimation != null && spineAnimation.AnimationState != null && spineAnimation.AnimationState.Data != null)
            {
                spineAnimation.AnimationState.Data.SetMix(ANIM_JUMPSTART, ANIM_JUMPRISE, 0.05f);
                spineAnimation.AnimationState.Data.SetMix(ANIM_JUMPRISE, ANIM_JUMPFALL, 0.1f);
                spineAnimation.AnimationState.Data.SetMix(ANIM_JUMPFALL, ANIM_JUMPLAND, 0.05f);
                spineAnimation.AnimationState.Data.SetMix(ANIM_JUMPLAND, ANIM_IDLE, 0.15f);
                spineAnimation.AnimationState.Data.SetMix(ANIM_IDLE, ANIM_HURT, 0.05f);
            }

            if (playerHealth != null)
            {
                playerHealth.OnDamageTaken += PlayHurtAnimation;
                playerHealth.OnDeath += OnPlayerDeath; // lock input immediately on death
            }

            if (playerAttack != null)
            {
                playerAttack.OnAttackPerformed += PlayerAttackAnimation;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDamageTaken -= PlayHurtAnimation;
                playerHealth.OnDeath -= OnPlayerDeath;
            }

            if (playerAttack != null)
            {
                playerAttack.OnAttackPerformed -= PlayerAttackAnimation;
            }
        }

        void Update()
        {
            if (inputLocked || isDead) return;

            GatherInput();
            CheckGround();
            HandleJumpBuffer();
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            if (!isKnockback)
            {
                HandleVerticalMovement();
            }

            if (!animationLocked && !isKnockback)
            {
                HandleHorizontalMovement();
            }

            ApplyMovement();
            UpdateAnimationState();

            // fixed animation debug
            if (spineAnimation != null)
            {
                if (spineAnimation.AnimationName != lastAnimName)
                {
                    Debug.Log($"[AnimChange] {lastAnimName} -> {spineAnimation.AnimationName}");
                    lastAnimName = spineAnimation.AnimationName;
                }
            }

            // update airborne peak tracking every physics frame
            if (isTrackingAirborne)
            {
                float currentY = transform.position.y;
                if (currentY > airbornePeakY) airbornePeakY = currentY;

                float absXDelta = Mathf.Abs(transform.position.x - airborneStartPos.x);
                if (absXDelta > airborneMaxHorizontalDelta) airborneMaxHorizontalDelta = absXDelta;
            }
        }

        private void GatherInput()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isOnGround || coyoteTimer > 0)
                {
                    jumpBufferTimer = jumpBufferTime;
                }
                else if (hasDoubleJump && doubleJumpRemaining > 0)
                {
                    ExecuteDoubleJump();
                }
            }

            // If player released jump button while jumping
            if (Input.GetKeyUp(KeyCode.Space) && isJumping && frameVelocity.y > 0)
            {
                frameVelocity.y /= jumpCutMultiplier;
                isJumping = false;
            }
        }

        private void CheckGround()
        {
            if (isDead) return;

            bool wasOnGround = isOnGround;
            isOnGround = Physics2D.OverlapCircle(groundPoint.position, .2f, whatIsGround);

            // just landed from jumping or falling
            if (!wasOnGround && isOnGround)
            {
                // play land anim and then idle
                var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPLAND, false);
                entry.Complete += (te) =>
                {
                    if (!isDead)
                        spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_IDLE, true);
                };

                coyoteTimer = 0f;
                isJumping = false;
                doubleJumpRemaining = maxDoubleJump; // reset double jump

                if (isTrackingAirborne)
                {
                    EndAirborneTracking();
                }
            }
            // just left ground
            else if (wasOnGround && !isOnGround)
            {
                coyoteTimer = coyoteTime;
            }

            if (coyoteTimer > 0f) coyoteTimer -= Time.deltaTime;
        }

        private void HandleJumpBuffer()
        {
            if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime;
        }

        private void HandleHorizontalMovement()
        {
            float targetSpeed = horizontalInput * maxHorizontalSpeed;
            float currentSpeed = frameVelocity.x;

            // accelerate
            if (horizontalInput != 0f)
                accelerationRate = horizontalAcceleration;
            else
            {
                // decelerate
                accelerationRate = isOnGround ? groundFriction : airFriction;
                targetSpeed = 0;
            }

            // smooth speed transform
            frameVelocity.x = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.fixedDeltaTime);

            // player direction
            if (!isKnockback)
            {
                if (horizontalInput < 0)
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                else if (horizontalInput > 0)
                    transform.localScale = Vector3.one;
            }
        }

        private void HandleVerticalMovement()
        {
            // jump check
            if (jumpBufferTimer > 0 && (isOnGround || coyoteTimer > 0))
            {
                ExecuteJump();
            }

            // add a tiny downward force to make sure collider check is stable
            if (isOnGround)
            {
                if (frameVelocity.y < 0) frameVelocity.y = -0.5f;
            }

            bool isHoldingJump = Input.GetKey(KeyCode.Space);
            float currentGravity = riseGravity; // base gravity when rising

            if (frameVelocity.y > 0)
            {
                // release early, stronger gravity
                currentGravity = isHoldingJump ? riseGravity : (riseGravity * jumpCutMultiplier);
            }
            else if (frameVelocity.y < 0)
            {
                currentGravity = riseGravity * fallingMultiplier;
            }

            float currentYSpeed = frameVelocity.y;
            float targetYSpeed = -maxFallSpeed;
            float fallMaxDelta = currentGravity * Time.fixedDeltaTime;
            frameVelocity.y = Mathf.MoveTowards(currentYSpeed, targetYSpeed, fallMaxDelta);
        }

        private void ExecuteJump()
        {
            frameVelocity.y = jumpPower;
            jumpBufferTimer = 0;
            coyoteTimer = 0f;
            isJumping = true;

            JumpStartAnimation();

            BeginAirborneTrackingIfNeeded();
        }

        private void ExecuteDoubleJump()
        {
            frameVelocity.y = Mathf.Max(frameVelocity.y, doubleJumpPower);
            doubleJumpRemaining--;
            isJumping = true;

            DoubleJumpAnimation();

            BeginAirborneTrackingIfNeeded();
        }

        private void ApplyMovement()
        {
            RB.linearVelocity = frameVelocity;
        }

        public void LockInput()
        {
            inputLocked = true;
        }

        public void UnlockInput()
        {
            inputLocked = false;
        }

        public void IdleAnimation()
        {
            if (spineAnimation.AnimationName != ANIM_IDLE)
                spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_IDLE, true);
        }

        public void RunAnimation()
        {
            if (spineAnimation.AnimationName != ANIM_RUN)
                spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_RUN, true);
        }

        private void JumpStartAnimation()
        {
            if (spineAnimation == null || isDead) return;
            var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPSTART, false);
            entry.Complete += (te) =>
            {
                if (isDead) return;
                if (frameVelocity.y > 0.1f)
                    spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPRISE, true);
                else
                    spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPFALL, true);
            };
        }

        private void JumpFallAnimation()
        {
            if (isDead) return;
            spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPFALL, true);
        }

        private void DoubleJumpAnimation()
        {
            if (spineAnimation == null || isDead) return;
            // intentionally disabled double jump anim setup here
        }

        private void UpdateAnimationState()
        {
            if (spineAnimation == null) return;
            if (animationLocked || isDead) return;

            if (spineAnimation.AnimationName == ANIM_JUMPLAND && Mathf.Abs(horizontalInput) > 0.1f)
            {
                playingJumpLand = false;
                spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_RUN, true);
                return;
            }

            if (playingJumpLand)
                return;

            float horizontalSpeed = Mathf.Abs(frameVelocity.x);

            // airborne
            if (!isOnGround)
            {
                if (frameVelocity.y > 0.1f)
                {
                    if (spineAnimation.AnimationName != ANIM_JUMPRISE)
                        spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPRISE, true);
                }
                else if (frameVelocity.y < -0.1f)
                {
                    if (spineAnimation.AnimationName != ANIM_JUMPFALL)
                        JumpFallAnimation();
                }
                return;
            }

            // grounded
            if (spineAnimation.AnimationName == ANIM_JUMPLAND)
                return;

            if (horizontalSpeed > 0.1f)
            {
                if (spineAnimation.AnimationName != ANIM_RUN)
                    RunAnimation();
            }
            else
            {
                if (spineAnimation.AnimationName != ANIM_IDLE)
                    IdleAnimation();
            }
        }

        private void LockAnimation(float duration)
        {
            animationLocked = true;
            CancelInvoke(nameof(UnlockAnimation));
            Invoke(nameof(UnlockAnimation), duration);
        }

        private void UnlockAnimation()
        {
            animationLocked = false;
        }

        public void PlayerAttackAnimation()
        {
            if (animationLocked || isDead) return;
            if (spineAnimation == null) return;

            LockAnimation(0.25f);

            var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, "attack", false);
            entry.Complete += (e) =>
            {
                if (isDead) return;
                UnlockAnimation();
                spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_IDLE, true);
            };
        }

        public void PlayHurtAnimation()
        {
            if (animationLocked || isDead) return;
            if (spineAnimation == null) return;
            LockAnimation(0.25f);
            var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_HURT, false);
            entry.Complete += (e) =>
            {
                if (isDead) return;
                UnlockAnimation();
                spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_IDLE, true);
            };
        }

        public void ApplyKnockback(Vector2 sourcePosition)
        {
            if (isDead) return;

            isKnockback = true;
            float dirX = transform.position.x - sourcePosition.x;
            float sign = Mathf.Sign(dirX);
            if (sign == 0) sign = transform.localScale.x >= 0 ? 1f : -1f; //fallback

            Vector2 initialForce = new Vector2(sign * knockbackHorizontal, knockbackVertical);

            StartCoroutine(KnockbackCoroutine(initialForce, knockbackDuration));
        }

        private IEnumerator KnockbackCoroutine(Vector2 initialForce, float duration)
        {
            inputLocked = true;
            LockAnimation(duration);

            float timer = 0f;
            frameVelocity.x = initialForce.x;
            frameVelocity.y = Mathf.Max(frameVelocity.y, initialForce.y);

            while (timer < duration)
            {
                yield return new WaitForFixedUpdate();
                timer += Time.fixedDeltaTime;

                float targetX = 0f;
                frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, targetX, knockbackHorizontalDecay * Time.fixedDeltaTime);

                float gravityThisFrame;
                if (frameVelocity.y > 0)
                    gravityThisFrame = riseGravity * Time.fixedDeltaTime;
                else
                    gravityThisFrame = riseGravity * fallingMultiplier * Time.fixedDeltaTime;

                frameVelocity.y -= gravityThisFrame;

                // clamp vertical fall speed
                if (frameVelocity.y < -maxFallSpeed) frameVelocity.y = -maxFallSpeed;


                // apply to rigidbody so physics & collisions update visually
                RB.linearVelocity = frameVelocity;
            }
            inputLocked = false;
            isKnockback = false;
        }



        // invincible frame flashing flicker
        public void StartInvincibleFlicker(float duration, float flickerInterval = 0.1f)
        {
            if (flickerRoutine != null)
                StopCoroutine(flickerRoutine);
            flickerRoutine = StartCoroutine(FlickerCoroutine(duration, flickerInterval));
        }

        private IEnumerator FlickerCoroutine(float duration, float flickerInterval)
        {
            float timer = 0f;
            bool fadingOut = true;

            var skeleton = spineAnimation.Skeleton;
            float startAlpha, endAlpha;

            while (timer < duration)
            {
                float fadeTimer = 0f;
                startAlpha = fadingOut ? 1f : 0.5f;
                endAlpha = fadingOut ? 0.5f : 1f;

                while (fadeTimer < flickerInterval)
                {
                    fadeTimer += Time.deltaTime;
                    float t = fadeTimer / flickerInterval;
                    float newAlpha = Mathf.Lerp(startAlpha, endAlpha, t);
                    skeleton.A = newAlpha;
                    yield return null;
                }

                fadingOut = !fadingOut;
                timer += flickerInterval;
            }

            skeleton.A = 1f;
            flickerRoutine = null;
        }

        public IEnumerator PlayDeathAndWait()
        {
            if (spineAnimation == null) yield break;

            frameVelocity.x = 0f;
            RB.linearVelocity = new Vector2(0f, RB.linearVelocity.y);

            LockAnimation(999f); // keep it locked until respawn

            bool finished = false;
            var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, "death", false);
            entry.Complete += (te) =>
            {
                finished = true;
            };

            yield return new WaitUntil(() => finished);
        }

        private void OnDrawGizmos()
        {
            if (groundPoint != null)
            {
                Gizmos.color = isOnGround ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundPoint.position, .2f);
            }
        }

        private void BeginAirborneTrackingIfNeeded()
        {
            if (!enableJumpLogging) return;

            if (!isTrackingAirborne)
            {
                isTrackingAirborne = true;
                airborneStartTime = Time.time;
                airborneStartPos = transform.position;
                airbornePeakY = transform.position.y;
                airborneMaxHorizontalDelta = 0f;
            }
        }

        private void EndAirborneTracking()
        {
            if (!enableJumpLogging)
            {
                isTrackingAirborne = false;
                return;
            }

            isTrackingAirborne = false;
            float airtime = Time.time - airborneStartTime;
            float peakHeight = airbornePeakY - airborneStartPos.y;
            float landingXDisplacement = transform.position.x - airborneStartPos.x;
            float maxHorizontalDelta = airborneMaxHorizontalDelta;

            JumpRecord rec = new JumpRecord
            {
                startTime = airborneStartTime,
                airtime = airtime,
                peakHeight = peakHeight,
                horizontalDisplacementLanding = landingXDisplacement,
                maxHorizontalDelta = maxHorizontalDelta,
                startPos = airborneStartPos,
                landingPos = transform.position
            };

            jumpRecords.Add(rec);

            Debug.Log($"[JumpStats #{jumpRecords.Count}] airtime={airtime:F3}s | peakHeight={peakHeight:F3}u | landingHorizontal={landingXDisplacement:F3}u | maxHorizontalDelta={maxHorizontalDelta:F3}u | startY={airborneStartPos.y:F3} | peakY={airbornePeakY:F3}");
        }

        private void OnPlayerDeath()
        {
            // mark dead first to gate all logic
            isDead = true;

            // immediately stop control and movement when dead
            inputLocked = true;
            animationLocked = true;
            CancelInvoke(nameof(UnlockAnimation));

            horizontalInput = 0f;
            frameVelocity = Vector2.zero;

            // stop local coroutines that could still modify velocity/animation (knockback/flicker)
            StopAllCoroutines();
            isKnockback = false;

            if (RB != null)
            {
                // kill horizontal motion now and prevent any further X movement from physics
                RB.linearVelocity = new Vector2(0f, RB.linearVelocity.y);
                RB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }

            if (playerAttack != null)
            {
                playerAttack.enabled = false;
            }

            // clear any pending animation callbacks and force death animation
            if (spineAnimation != null && spineAnimation.AnimationState != null)
            {
                // reset any flicker alpha
                if (spineAnimation.Skeleton != null) spineAnimation.Skeleton.A = 1f;

                spineAnimation.AnimationState.ClearTrack(TRACK_INDEX);
                spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_DEATH, false);
                lastAnimName = ANIM_DEATH;
            }
        }

        public void ResetPhysicsAfterRespawn()
        {
            if (RB != null)
            {
                RB.constraints = RigidbodyConstraints2D.FreezeRotation; // unfreeze X, keep rotation frozen
                RB.linearVelocity = Vector2.zero;
            }
        }
    }
}
