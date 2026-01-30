using UnityEngine;
using System;

public class EnemyController : MonoBehaviour
{   
    // Enemy Stats
    public int enemyHealth = 3;
    private Animator animator;
    public float moveSpeed = 1.5f;
    public Transform groundCheckPoint;
    public float distance = .3f;
    public LayerMask groundLayer;
    private bool facingLeft;

    // Chasing Player
    public float chaseRangeRadius = 6f;
    public LayerMask playerLayer;
    public Transform player;
    public float chaseSpeed = 2f;
    public float retrieveDistance = 3f;
   
    // Components
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    public GameObject floatingTextPrefab;
    public Transform textSpawnPoint;
    public Transform attackPoint;
    public float attackRadius = 1f;
    
    public float patrolRange = 5f;  
    private Vector3 startPosition;
    
    void Start()
    {
        animator = this.gameObject.GetComponent<Animator>();
        facingLeft = true;
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        boxCollider2D = this.gameObject.GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyHealth <= 0)
        {
            Die();
        }

        if (player != null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        PlayerDetection();
        EnemyRunAnimation();

    }
    public void PlayerDetection()
    {
        // OverlapCircle để kiểm tra xem có player hay không
        Collider2D collInfo = Physics2D.OverlapCircle(transform.position, chaseRangeRadius, playerLayer);
        // Raycast để kiểm tra xem có ground hay không
        RaycastHit2D hitInfo = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, distance, groundLayer);
        if (collInfo)
        {
            // Nếu không có ground khi chasing thì đứng lại, idle, xoay hướng về phía player
            if (hitInfo == false)
            {
                // Xoay hướng về phía player
                if (player.position.x > transform.position.x && facingLeft)
                {
                    transform.eulerAngles = new Vector3(0, -180, 0);
                    facingLeft = false;
                }
                else if (player.position.x < transform.position.x && !facingLeft)
                {
                    transform.eulerAngles = new Vector3(0, 0, 0);
                    facingLeft = true;
                }
                // Không di chuyển nữa, chỉ đứng nhìn
            }
            else
            {
                // Có ground thì chase như bình thường
                // Face the player
                if (player.position.x > transform.position.x && facingLeft)
                {
                    transform.eulerAngles = new Vector3(0, -180, 0);
                    facingLeft = false;
                }
                else if (player.position.x < transform.position.x && !facingLeft)
                {
                    transform.eulerAngles = new Vector3(0, 0, 0);
                    facingLeft = true;
                }
                // Chase the player
                Vector2 targetPos = new Vector2(player.position.x, transform.position.y);
                if (Vector2.Distance(transform.position, targetPos) > retrieveDistance)
                {
                    animator.SetBool("Attack", false);
                    transform.position = Vector2.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
                }
                else
                {
                    animator.SetBool("Attack", true);
                }
            }
        }
        else
        {
            animator.SetBool("Attack", false);
            // Patrol
            transform.Translate(Vector2.left * Time.deltaTime * moveSpeed);
            // Nếu không có ground thì quay đầu lại
            if (hitInfo == false)
            {
                if (facingLeft)
                {
                    transform.eulerAngles = new Vector3(0, -180, 0);
                    facingLeft = false;
                }
                else
                {
                    transform.eulerAngles = new Vector3(0, 0, 0);
                    facingLeft = true;
                }
            }
            // Kiểm tra xem đã đi được quãng đường patrolRange chưa
            float distanceFromStart = Vector3.Distance(transform.position, startPosition);
            if (distanceFromStart >= patrolRange)
            {
                if (facingLeft)
                    {
                        transform.eulerAngles = new Vector3(0, -180, 0);
                        facingLeft = false;
                    }
                else
                    {   
                    transform.eulerAngles = new Vector3(0, 0, 0);
                    facingLeft = true;
                    }
                // reset lại gốc để nó tuần hoàn
                startPosition = transform.position;
            }
            
        }
    }
    
    public void Attack()
    {
        Collider2D collInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);

        if (collInfo)
        {
            if(collInfo.gameObject.GetComponent<PlayerController>() != null)
            {
                PlayerController.instance.TakeDamage(1);
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if(enemyHealth <= 0)
        {
            return;
        }
        enemyHealth -= damageAmount;
        animator.SetTrigger("GetHit");
        CameraShake.instance.Shake(2.5f, 0.15f);
        Instantiate(floatingTextPrefab, textSpawnPoint.position, Quaternion.identity);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerArrow"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
    
    public void ShakeCamera()
    {
        CameraShake.instance.Shake(4f, 0.18f);
    }

    private void OnDrawGizmosSelected()
    {
        if(groundCheckPoint == null)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawRay(groundCheckPoint.position, Vector2.down * distance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRangeRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

        // Vẽ tầm patrol
        Gizmos.color = Color.yellow;
        Vector3 leftBound = Application.isPlaying ? 
            new Vector3(startPosition.x - patrolRange, startPosition.y, startPosition.z): 
            new Vector3(transform.position.x - patrolRange, transform.position.y, transform.position.z);
        Vector3 rightBound = Application.isPlaying ? 
            new Vector3(startPosition.x + patrolRange, startPosition.y, startPosition.z) : 
            new Vector3(transform.position.x + patrolRange, transform.position.y, transform.position.z);
        Gizmos.DrawLine(leftBound, rightBound);
        Gizmos.DrawWireSphere(leftBound, 0.1f);
        Gizmos.DrawWireSphere(rightBound, 0.1f);

        // Vẽ đường retrieveDistance theo hướng player
        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 enemyPos = transform.position;
            Vector3 targetPos = new Vector3(player.position.x, enemyPos.y, enemyPos.z);
            Vector3 dirToPlayer = (targetPos - enemyPos).normalized;

            Gizmos.DrawLine(enemyPos, enemyPos + dirToPlayer * retrieveDistance);
        }
    }

    void EnemyRunAnimation()
    {
        if (Math.Abs(moveSpeed) > 0)
        {
            animator.SetFloat("WalkSpeed", Mathf.Abs(moveSpeed));
        }
    }
}
