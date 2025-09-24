using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatroller : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;      // Patrol waypoints (enemy will move between them)
    private int currentPoint = 0;         // Index of the current patrol point
    public float moveSpeed = 2f;          // Speed during patrol
    public float waitAtPoints = 1f;       // How long to wait at each point
    private float waitCounter;            // Internal timer for waiting

    [Header("Physics")]
    public float jumpForce = 6f;          // Force applied when jumping
    public Rigidbody2D theRB;             // Reference to the Rigidbody2D component
    public Animator anim;                 // Reference to the Animator component

    [Header("Player Detection / Chase")]
    public Transform player;              // Reference to the Player (assign in Inspector)
    public float chaseRadius = 3f;        // Distance at which enemy starts chasing the player
    public float chaseSpeed = 3.5f;       // Speed while chasing
    public float maxChaseDistance = 6f;   // Maximum distance from spawn point the enemy can chase
    public float losePlayerRadius = 4.5f; // Distance at which the enemy stops chasing if player is too far

    private bool isChasing = false;       // Is the enemy currently chasing the player?
    private Vector2 spawnPosition;        // Where the enemy started (used to limit chase distance)

    void Start()
    {
        waitCounter = waitAtPoints;
        spawnPosition = transform.position; // Save spawn position

        // Detach patrol points from enemy object
        if (patrolPoints != null)
        {
            foreach (Transform pPoint in patrolPoints)
            {
                if (pPoint != null)
                    pPoint.SetParent(null);
            }
        }
    }

    void Update()
    {
        // Auto-assign player if not set (looks for GameObject with "Player" tag)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Distance from enemy to player
        float playerDist = (player != null) ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;

        // If player is within chase radius, start chasing
        if (!isChasing && player != null && playerDist <= chaseRadius)
        {
            isChasing = true;
        }

        if (isChasing && player != null)
        {
            // Distance from spawn point (to limit chase area)
            float fromSpawn = Vector2.Distance(spawnPosition, transform.position);

            // Stop chasing if player is too far or enemy has left max chase area
            if (fromSpawn > maxChaseDistance || (playerDist > losePlayerRadius && playerDist > chaseRadius))
            {
                isChasing = false;
                currentPoint = FindClosestPatrolPointIndex(); // Resume patrol near closest point
                waitCounter = waitAtPoints;
            }
            else
            {
                // Move horizontally towards the player
                float dir = Mathf.Sign(player.position.x - transform.position.x);
                theRB.linearVelocity = new Vector2(dir * chaseSpeed, theRB.linearVelocity.y);

                // Flip sprite to face movement direction
                transform.localScale = new Vector3(dir > 0 ? -1f : 1f, 1f, 1f);

                // Jump if player is above enemy and horizontally close
                if (transform.position.y < player.position.y - 0.5f && theRB.linearVelocity.y < 1f)
                {
                    theRB.linearVelocity = new Vector2(theRB.linearVelocity.x, jumpForce);
                }
            }
        }
        else
        {
            // Patrol logic (if patrol points exist)
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                // No patrol points �� stand still
                theRB.linearVelocity = new Vector2(0f, theRB.linearVelocity.y);
            }
            else
            {
                // Move towards current patrol point
                if (Mathf.Abs(transform.position.x - patrolPoints[currentPoint].position.x) > 0.2f)
                {
                    if (transform.position.x < patrolPoints[currentPoint].position.x)
                    {
                        theRB.linearVelocity = new Vector2(moveSpeed, theRB.linearVelocity.y);
                        transform.localScale = new Vector3(-1f, 1f, 1f);
                    }
                    else
                    {
                        theRB.linearVelocity = new Vector2(-moveSpeed, theRB.linearVelocity.y);
                        transform.localScale = new Vector3(1f, 1f, 1f);
                    }

                    // Jump if patrol point is above enemy
                    if (transform.position.y < patrolPoints[currentPoint].position.y - 0.5f && theRB.linearVelocity.y < 1f)
                    {
                        theRB.linearVelocity = new Vector2(theRB.linearVelocity.x, jumpForce);
                    }
                }
                else
                {
                    // Reached patrol point �� wait
                    theRB.linearVelocity = new Vector2(0f, theRB.linearVelocity.y);
                    waitCounter -= Time.deltaTime;
                    if (waitCounter <= 0f)
                    {
                        waitCounter = waitAtPoints;
                        currentPoint++;
                        if (currentPoint >= patrolPoints.Length) currentPoint = 0;
                    }
                }
            }
        }

        // Update animator parameter for movement speed
        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(theRB.linearVelocity.x));
    }

    // Find the closest patrol point index (used when resuming patrol after chasing)
    int FindClosestPatrolPointIndex()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return 0;
        int closest = 0;
        float best = Mathf.Infinity;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            float d = Vector2.Distance(transform.position, patrolPoints[i].position);
            if (d < best)
            {
                best = d;
                closest = i;
            }
        }
        return closest;
    }

    // Draw gizmos in Scene view for chase radius and max chase distance
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Gizmos.color = Color.red;
        Vector3 spawn = Application.isPlaying ? (Vector3)spawnPosition : transform.position;
        Gizmos.DrawWireSphere(spawn, maxChaseDistance);
    }
}
