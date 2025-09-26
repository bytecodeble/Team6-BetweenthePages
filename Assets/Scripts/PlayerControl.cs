using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private Rigidbody2D RB;
    private BoxCollider2D Collider;


    #region Movement Variables

    [Header("Walk")]
    [SerializeField] private float maxHorizontalSpeed = 7.5f;
    [SerializeField] private float horizontalAcceleration = 140;
    [SerializeField] private float groundFriction = 80;
    [SerializeField] private float airFriction = 20;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 20.25f;
    [SerializeField] private float maxFallSpeed = 50;
    [SerializeField] private float fallAcceleration = 50.625f;
    [SerializeField] private float jumpCutMultiplier = 2.5f; // The gravity multiplier added when jump is released early
    [SerializeField] private float fallingMultiplier = 2.2f; // make falling faster
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    private float accelerationRate; // this value will be calculated
    private bool isJumping;

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


    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        Collider = GetComponent<BoxCollider2D>();
        RB.gravityScale = 0; // We use our own calculations instead of the default gravity
    }

    void Update()
    {
        GatherInput();
        CheckGround();
        HandleJumpBuffer();
    }

    private void FixedUpdate()
    {
        // For physics logic
        HandleHorizontalMovement();
        HandleVerticalMovement();
        ApplyMovement();
    }

    private void GatherInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
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

        // Just landed from jumping or falling
        if (!wasOnGround && isOnGround)
        {
            coyoteTimer = 0f; // reset coyote timer
            isJumping = false;
        }

        // Just left the ground
        else if (wasOnGround && !isOnGround)
        {
            coyoteTimer = coyoteTime; // start coyote timer
        }

        if (coyoteTimer > 0f)
        {
            coyoteTimer -= Time.deltaTime;
        }

    }

    private void HandleJumpBuffer()
    {
        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }
    }

    private void HandleHorizontalMovement()
    {
        float targetSpeed = horizontalInput * maxHorizontalSpeed;

        float currentSpeed = frameVelocity.x;


        // accelerate
        if (horizontalInput != 0f) 
        {
            accelerationRate = horizontalAcceleration;
        }
        else
        {
            // Decelerate due to friction force
            accelerationRate = isOnGround ? groundFriction : airFriction;
            // If player stopped input, speed will approach 0
            targetSpeed = 0;
        }

        // smooth speed transform
        frameVelocity.x = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate);

        // player direction
        if (frameVelocity.x < 0f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (frameVelocity.x > 0f)
        {
            transform.localScale = Vector3.one;
        }
    }

    private void HandleVerticalMovement()
    {
        // Jump check, prioritize checking jump buffer timer, then grounded or coyote timer
        if(jumpBufferTimer > 0 && (isOnGround || coyoteTimer > 0))
        {
            ExecuteJump();
        }

        // If grounded, add a tiny downward force to make sure collider check is stable
        if (isOnGround) 
        {
            if (frameVelocity.y < 0)
            {
                frameVelocity.y = -0.5f;
            }

        }


        bool isHoldingJump = Input.GetKey(KeyCode.Space);
        float currentGravity = fallAcceleration; // base gravity when rising


        if (frameVelocity.y > 0 )
        {
            if (!isHoldingJump)
            {
                // release early, stronger gravity
                currentGravity = fallAcceleration * jumpCutMultiplier;
            }
            else
            {
                currentGravity = fallAcceleration;
            }
        }
        else if (frameVelocity.y < 0)
        {
            // make gravity stronger than rising
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
        coyoteTime = 0;
        isJumping = true;
    }

    private void ApplyMovement()
    {
        RB.linearVelocity = frameVelocity;
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
