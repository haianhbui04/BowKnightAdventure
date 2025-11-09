using UnityEngine;

public class Enemy2Controller : MonoBehaviour
{
    public int maxHealth = 3;
    private bool facingLeft;

    public float attackRangeRadius = 6f;
    public LayerMask playerLayer;
    public Transform player;
    private Animator animator;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    public GameObject floatingTextPrefab;
    public Transform textSpawnPoint;

    [Header("Attack")]
    public Transform firePoint;
    public GameObject arrowPrefab;
    public float arrowVelocity = 10f;
    private bool isPlayerInAttackRange;
    public Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        facingLeft = true;
        isPlayerInAttackRange = false;
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        animator = this.gameObject.GetComponent<Animator>();
        boxCollider2D = this.gameObject.GetComponent<BoxCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            animator.SetBool("Attack", false);
        }

        if(player != null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        

        if (maxHealth <= 0)
        {

            Die();
            return;
        }

        Collider2D collInfo = Physics2D.OverlapCircle(transform.position + offset, attackRangeRadius, playerLayer);

        if (collInfo)
        {
            isPlayerInAttackRange = true;
            animator.SetBool("Attack", true);
        }
        else
        {
            isPlayerInAttackRange = false;
            animator.SetBool("Attack", false);
        }

        if (isPlayerInAttackRange)
        {
            if(transform.position.x < player.position.x && facingLeft)
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
                facingLeft = false;
            }
            else if (transform.position.x > player.position.x && !facingLeft)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                facingLeft = true;
            }
        }
    }

    public void FireArrow()
    {
        if (isPlayerInAttackRange)
        {
            GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            
            arrowRb.linearVelocity = -arrow.transform.right * arrowVelocity;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if(maxHealth <= 0)
        {
            return;
        }
        maxHealth -= damageAmount;
        animator.SetTrigger("GetHit");
        CameraShake.instance.Shake(2.5f, 0.15f);
        Instantiate(floatingTextPrefab, textSpawnPoint.position, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerArrow"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
    void Die()
    {
        animator.SetBool("Dead", true);
        this.enabled = false;
        // Disable collider and gravity
        rb.gravityScale = 0;
        boxCollider2D.enabled = false;

        CameraShake.instance.Shake(2.5f, 0.15f);
        Destroy(this.gameObject, 5f);

    }
    
     private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRangeRadius);
    }
}
