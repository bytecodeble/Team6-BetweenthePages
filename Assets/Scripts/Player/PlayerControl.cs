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
        private float groundFriction = 80;
        private float airFriction = 20;

        [Header("Jump")]
        private float jumpPower = 20.25f;
        private float maxFallSpeed = 35f;
        private float fallAcceleration = 25f;
        private float jumpCutMultiplier = 2.5f; // gravity multiplier added when jump is released early
        private float fallingMultiplier = 2f; // make falling faster
        private float coyoteTime = 0.2f;
        private float jumpBufferTime = 0.15f;

        [Header("Double Jump")]
        private float doubleJumpPower = 18.25f;
        private int maxDoubleJump = 0;

        private float accelerationRate;
        private bool isJumping;
        private int doubleJumpRemaining;
        [HideInInspector] public bool hasDoubleJump = false; // TODO: change this with actual ability upgrade function
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
        private const string ANIM_DOUBLEJUMPRISE = "double_jump";
        private const string ANIM_ATTACK = "attack";
        private const string ANIM_HURT = "hurt";
        private const string ANIM_DEATH = "death";

        // flickers
        private Coroutine flickerRoutine;
        private Color originalColor;
        private MeshRenderer spineRenderer;


        // animation debugger
        private string lastAnimName = "";
        private bool playingJumpStart = false;
        private bool playingDoubleJump = false;
        private bool playingJumpLand = false;


        #endregion



        private void Awake()
        {
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

            playerHealth.OnDamageTaken += PlayHurtAnimation;
            playerAttack.OnAttackPerformed += PlayerAttackAnimation;


        }

        void Update()
        {
            if (inputLocked) return;

            GatherInput();
            CheckGround();
            HandleJumpBuffer();
        }

        private void FixedUpdate()
        {
            if (!animationLocked)
            {
                HandleHorizontalMovement();
                HandleVerticalMovement();
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
        }

        private void GatherInput()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!isOnGround && doubleJumpRemaining > 0 && hasDoubleJump)
                {
                    ExecuteDoubleJump();
                }
                else
                {
                    jumpBufferTimer = jumpBufferTime;
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
            bool wasOnGround = isOnGround;
            isOnGround = Physics2D.OverlapCircle(groundPoint.position, .2f, whatIsGround);

            // just landed from jumping or falling
            if (!wasOnGround && isOnGround)
            {
                // play land anim and then idle
                var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPLAND, false);
                entry.Complete += (te) =>
                {
                    spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_IDLE, true);
                };

                coyoteTimer = 0f;
                isJumping = false;
                doubleJumpRemaining = maxDoubleJump; // reset double jump
                playingJumpStart = false;
                playingDoubleJump = false;
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
            if (frameVelocity.x < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
            else if (frameVelocity.x > 0f) transform.localScale = Vector3.one;
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
            float currentGravity = fallAcceleration; // base gravity when rising

            if (frameVelocity.y > 0)
            {
                // release early, stronger gravity
                currentGravity = isHoldingJump ? fallAcceleration : (fallAcceleration * jumpCutMultiplier);
            }
            else if (frameVelocity.y < 0)
            {
                currentGravity = fallAcceleration * fallingMultiplier;
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
        }

        private void ExecuteDoubleJump()
        {
            frameVelocity.y = Mathf.Max(frameVelocity.y, doubleJumpPower);
            doubleJumpRemaining--;
            isJumping = true;

            DoubleJumpAnimation();
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
            if (spineAnimation == null) return;
            playingJumpStart = true;
            var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPSTART, false);
            entry.Complete += (te) =>
            {
                playingJumpStart = false;
                // if still moving up, go to rise, otherwise fall
                if (frameVelocity.y > 0.1f)
                    spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPRISE, true);
                else
                    spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPFALL, true);
            };
        }

        private void JumpFallAnimation()
        {
            // cancel any jumpstart or doublejump playing flags so falling takes over
            playingJumpStart = false;
            playingDoubleJump = false;
            spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPFALL, true);
        }


        private void DoubleJumpAnimation()
        {
            if (spineAnimation == null) return;
            playingDoubleJump = true;
            var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_DOUBLEJUMPRISE, false);
            entry.Complete += (te) =>
            {
                playingDoubleJump = false;
                if (frameVelocity.y > 0.1f)
                    spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPRISE, true);
                else
                    spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPFALL, true);
            };
        }

        private void UpdateAnimationState()
        {
            if (spineAnimation == null) return;

            if (animationLocked) return;

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
            if (spineAnimation == null) return;
            LockAnimation(0.4f);
            var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, "attack", false);
            entry.Complete += (e) =>
            {
                UnlockAnimation();
                spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_IDLE, true);
            };
        }

        public void PlayHurtAnimation()
        {
            if (spineAnimation == null) return;
            LockAnimation(0.25f);
            var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_HURT, false);
            entry.Complete += (e) =>
            {
                UnlockAnimation();
                spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_IDLE, true);
            };
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


    }
}

