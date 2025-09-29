using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    //animation for sword
    private Animator animator;
    [SerializeField] private GameObject sword; 

    private Collider2D swordCollider;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (sword != null)
        {
            swordCollider = sword.GetComponent<Collider2D>();
            //When not in attack mode, the sword's collider is closed.
            swordCollider.enabled = false; 
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) 
        {
            animator.SetTrigger("Attack");
        
        }
    }

    //The attack animation now has three frames, and the collider is turned on after the sword is swung.
    public void EnableSword()
    {
        if (swordCollider != null)
            swordCollider.enabled = true;
    }

    //The last frame of the sword swing is closed by collider
    public void DisableSword()
    {
        if (swordCollider != null)
            swordCollider.enabled = false;
    }

    //attack object"Enemy", enemy distory
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
        }
    }
}
