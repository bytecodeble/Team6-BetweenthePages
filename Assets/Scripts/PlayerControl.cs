using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private Rigidbody2D RB;

    [SerializeField]private float walkSpeed;
    [SerializeField] private float jumpForce;

    [SerializeField] private Transform groundPoint;
    [SerializeField] private bool isOnGround;
    [SerializeField] private LayerMask whatIsGround;
    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
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
