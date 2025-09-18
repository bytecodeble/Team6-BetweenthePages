using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public Rigidbody2D RB;

    public float walkSpeed;
    public float jumpForce;

    public Transform groundPoint;
    private bool isOnGround;
    public LayerMask whatIsGround;
    void Start()
    {
        
    }

    void Update()
    {
        RB.linearVelocity = new Vector2(Input.GetAxisRaw("Horizontal") * walkSpeed,RB.linearVelocity.y);

        //player change direction
        if(RB.linearVelocity.x < 0)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }else if(RB.linearVelocity.x > 0)
        {
            transform.localScale = Vector3.one;
        }

        //check if the player on the ground,if true, jumping
        isOnGround = Physics2D.OverlapCircle(groundPoint.position, .2f , whatIsGround);

        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            RB.linearVelocity = new Vector2(RB.linearVelocity.x,jumpForce);
        }
    }
}
