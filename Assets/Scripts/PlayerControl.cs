using Spine.Unity;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private Rigidbody2D RB;
    private BoxCollider2D Collider;

    #region Movement Variables
    [Header("Walk")]
    private float maxHorizontalSpeed = 7.5f;
    private float horizontalAcceleration = 140;
    private float groundFriction = 80;
    private float airFriction = 20;

    [Header("Jump")]
    private float jumpPower = 20.25f;
    private float maxFallSpeed = 50;
    private float fallAcceleration = 50.625f;
    private float jumpCutMultiplier = 2.5f;
    private float fallingMultiplier = 2.2f;
    private float coyoteTime = 0.2f;
    private float jumpBufferTime = 0.15f;

    [Header("Double Jump")]
    private float doubleJumpPower = 18.25f;
    private int maxDoubleJump = 0;

    private float accelerationRate;
    private bool isJumping;
    private int doubleJumpRemaining;
    [HideInInspector] public bool hasDoubleJump = false;
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

    private const string ANIM_IDLE = "idle";
    private const string ANIM_RUN = "run";
    private const string ANIM_JUMPSTART = "jump_start";
    private const string ANIM_JUMPRISE = "jump_rise";
    private const string ANIM_JUMPFALL = "jump_fall";
    private const string ANIM_JUMPLAND = "jump_land";
    private const string ANIM_DOUBLEJUMPRISE = "double_jump";

    private const int TRACK_INDEX = 0;



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
        RB.gravityScale = 0;

        if (spineAnimation == null)
            spineAnimation = GetComponent<SkeletonAnimation>();

        if (spineAnimation != null && spineAnimation.AnimationState != null && spineAnimation.AnimationState.Data != null)
        {
            spineAnimation.AnimationState.Data.SetMix(ANIM_JUMPSTART, ANIM_JUMPRISE, 0.05f);
            spineAnimation.AnimationState.Data.SetMix(ANIM_JUMPRISE, ANIM_JUMPFALL, 0.1f);
            spineAnimation.AnimationState.Data.SetMix(ANIM_JUMPFALL, ANIM_JUMPLAND, 0.05f);
            spineAnimation.AnimationState.Data.SetMix(ANIM_JUMPLAND, ANIM_IDLE, 0.15f);
        }

        doubleJumpRemaining = maxDoubleJump;
    }

    void Update()
    {
        GatherInput();
        CheckGround();
        HandleJumpBuffer();
    }

    private void FixedUpdate()
    {
        HandleHorizontalMovement();
        HandleVerticalMovement();
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
            doubleJumpRemaining = maxDoubleJump;
            playingJumpStart = false;
            playingDoubleJump = false;
        }
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

        if (horizontalInput != 0f)
            accelerationRate = horizontalAcceleration;
        else
        {
            accelerationRate = isOnGround ? groundFriction : airFriction;
            targetSpeed = 0;
        }

        frameVelocity.x = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.fixedDeltaTime);

        if (frameVelocity.x < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
        else if (frameVelocity.x > 0f) transform.localScale = Vector3.one;
    }

    private void HandleVerticalMovement()
    {
        if (jumpBufferTimer > 0 && (isOnGround || coyoteTimer > 0))
        {
            ExecuteJump();
        }

        if (isOnGround)
        {
            if (frameVelocity.y < 0) frameVelocity.y = -0.5f;
        }

        bool isHoldingJump = Input.GetKey(KeyCode.Space);
        float currentGravity = fallAcceleration;

        if (frameVelocity.y > 0)
        {
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

    private void JumpLandAnimation()
    {
        if (spineAnimation == null) return;

        playingJumpLand = true;
        var entry = spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_JUMPLAND, false);
        entry.Complete += (te) =>
        {
            playingJumpLand = false;
            spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_IDLE, true);
        };
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



        if (spineAnimation.AnimationName == ANIM_JUMPLAND && Mathf.Abs(horizontalInput) > 0.1f)
        {
            playingJumpLand = false;
            spineAnimation.AnimationState.SetAnimation(TRACK_INDEX, ANIM_RUN, true);
            return;
        }

        if (playingJumpLand)
            return;



        float horizontalSpeed = Mathf.Abs(frameVelocity.x);

        // if we currently in a locked animation, skip auto transitions
        if (playingJumpStart || playingDoubleJump)
            return;

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

        // Prevent overriding the land animation before it finishes
        if (spineAnimation.AnimationName == ANIM_JUMPLAND)
            return;

        // Ground animations
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


    private void OnDrawGizmos()
    {
        if (groundPoint != null)
        {
            Gizmos.color = isOnGround ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundPoint.position, .2f);
        }
    }

    
}
