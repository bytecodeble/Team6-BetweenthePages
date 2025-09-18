using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    public GameObject sword; 

    private Collider2D swordCollider;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (sword != null)
        {
            swordCollider = sword.GetComponent<Collider2D>();
            swordCollider.enabled = false; 
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            animator.SetTrigger("Attack");
        
        }
    }
    public void EnableSword()
    {
        if (swordCollider != null)
            swordCollider.enabled = true;
    }

    public void DisableSword()
    {
        if (swordCollider != null)
            swordCollider.enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
        }
    }
}
