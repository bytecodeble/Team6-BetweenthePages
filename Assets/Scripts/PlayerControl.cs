using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

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
    private float jumpCutMultiplier = 2.5f; // The gravity multiplier added when jump is released early
    private float fallingMultiplier = 2.2f; // make falling faster
    private float coyoteTime = 0.2f;
    private float jumpBufferTime = 0.15f;

    [Header("Double Jump")]
    private float doubleJumpPower = 18.25f;
    private int maxDoubleJump = 0;

    private float accelerationRate; // this value will be calculated
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

    #region Jump Debugging / Tracking

    [Header("Debug Jump Stats")]
    [SerializeField] private bool enableJumpLogging = true; // toggle from Inspector
    // internal tracking for a whole airborne session (initial jump -> any double jumps -> landing)
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

        // update airborne peak tracking every physics frame (so it catches true maxY while in air)
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

        // Just landed from jumping or falling
        if (!wasOnGround && isOnGround)
        {
            // finalize airborne tracking if we were tracking a jump sequence
            if (isTrackingAirborne)
            {
                EndAirborneTracking();
            }

            coyoteTimer = 0f; // reset coyote timer
            isJumping = false;
            doubleJumpRemaining = maxDoubleJump; // reset double jump
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
        frameVelocity.x = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.fixedDeltaTime);

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
        if (jumpBufferTimer > 0 && (isOnGround || coyoteTimer > 0))
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


        if (frameVelocity.y > 0)
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
        coyoteTimer = 0f;
        isJumping = true;

        // start tracking the whole airborne sequence (initial jump -> possibly double jumps -> landing)
        BeginAirborneTrackingIfNeeded();
    }

    private void ExecuteDoubleJump()
    {
        frameVelocity.y = Mathf.Max(frameVelocity.y, doubleJumpPower);
        doubleJumpRemaining--;
        isJumping = true;

        // ensure that a tracking session is started even if initial jump was missed (e.g., fall + double jump)
        BeginAirborneTrackingIfNeeded();
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
        // compute and store stats for the just-finished airborne session
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

        if (enableJumpLogging)
        {
            Debug.Log($"[JumpStats #{jumpRecords.Count}] airtime={airtime:F3}s | peakHeight={peakHeight:F3}u | landingHorizontal={landingXDisplacement:F3}u | maxHorizontalDelta={maxHorizontalDelta:F3}u | startY={airborneStartPos.y:F3} | peakY={airbornePeakY:F3}");
        }
    }


    private void ApplyMovement()
    {
        RB.linearVelocity = frameVelocity; // changed to RB.velocity for clarity with physics
    }

    private void OnDrawGizmos()
    {
        if (groundPoint != null)
        {
            Gizmos.color = isOnGround ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundPoint.position, .2f);
        }
    }

    // Optional: call this from a debug UI or keystroke to dump CSV to persistentDataPath:
    // Example call: DumpJumpRecordsToCSV("MyJumpData.csv");
    public void DumpJumpRecordsToCSV(string filename = "JumpRecords.csv")
    {
        if (jumpRecords.Count == 0)
        {
            Debug.Log("[JumpStats] no records to export.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, filename);
        try
        {
            using (var sw = new StreamWriter(path, false))
            {
                sw.WriteLine("index,startTime,airtime,peakHeight,startX,startY,landingX,landingY,landingHorizontal,maxHorizontalDelta");
                for (int i = 0; i < jumpRecords.Count; i++)
                {
                    var r = jumpRecords[i];
                    sw.WriteLine($"{i + 1},{r.startTime:F3},{r.airtime:F3},{r.peakHeight:F3},{r.startPos.x:F3},{r.startPos.y:F3},{r.landingPos.x:F3},{r.landingPos.y:F3},{r.horizontalDisplacementLanding:F3},{r.maxHorizontalDelta:F3}");
                }
            }
            Debug.Log($"[JumpStats] Exported {jumpRecords.Count} records to: {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[JumpStats] Failed to export CSV: {ex.Message}");
        }
    }
}
