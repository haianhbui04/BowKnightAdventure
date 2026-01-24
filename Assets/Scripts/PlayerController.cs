using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{   
    // Player Stats
    public int playerHealth = 5;
    public static PlayerController instance;
    private Animator animator;
    private Rigidbody2D rb;
    // Movement
    public float orginalMoveSpeed = 5f;
    public float moveSpeed;
    public float jumpHeight = 10f;
    private float movement;
    private bool isGrounded;
    private bool facingRight;
    // Ground Check
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    // Attack
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float arrowSpeed = 7f;
    private BoxCollider2D boxCollider2D;
    // Collectibles
    private int currentDiamonds;
    public GameObject collectEffectPrefab;
    public Text curentHeartsText;
    public Text currentDiamondsText;
    //Player Dash
    public float dashDuration = .3f;
    private bool isDashing;
    public float dashForce = 15f;
    // Damage while touching hazards
    public float contactDamageInterval = 0.5f; // seconds between damage ticks while touching
    private float lastContactDamageTime = 0f;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isDashing = false;
        isGrounded = true;
        facingRight = true;
        moveSpeed = orginalMoveSpeed;
        animator = this.gameObject.GetComponent<Animator>();
        boxCollider2D = this.gameObject.GetComponent<BoxCollider2D>();
        rb = this.gameObject.GetComponent<Rigidbody2D>();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        currentDiamonds = 0;
       
    }

    // Update is called once per frame
    void Update()
    {
        // Update UI
        curentHeartsText.text = playerHealth.ToString();
        currentDiamondsText.text = currentDiamonds.ToString();
        // Check for death
        if (playerHealth <= 0)
        {
            Die();
        }

        // Ground check
        Collider2D collInfo = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        isGrounded = collInfo != null;
        if (isGrounded)
        {
            animator.SetBool("IsJumping", false);
        }

        // Dash input
        if (Input.GetMouseButtonDown(1) && !isDashing)
        {
            StartCoroutine(Dash());
        }

        Flip();
        PlayerRunAnimation();
        HandleMovement();
        HandleJump();
        HandleAttack();
        
    }

    private void FixedUpdate()
    {
        transform.position += new Vector3(movement * moveSpeed, 0, 0) * Time.fixedDeltaTime;
    }

    public void FireArrow()
    {
        GameObject tempArrowPrefab = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
        tempArrowPrefab.GetComponent<Rigidbody2D>().linearVelocity = arrowSpawnPoint.right * arrowSpeed;
    }

    void PlayerRunAnimation()
    {
        if (Math.Abs(movement) > 0)
        {
            animator.SetFloat("Run", Mathf.Abs(movement));
        }
    }

    void HandleMovement()
    {
        if (animator.GetBool("IsAttacking"))
        {
            return;
        } // Đang attack thì không được chạy
        movement = Input.GetAxis("Horizontal");
        animator.SetBool("IsRunning", movement != 0);
    }

    void HandleJump()
    {
        if (animator.GetBool("IsAttacking"))
        {
            return;
        }  // Đang attack thì không được nhảy

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            isGrounded = false;
            animator.SetBool("IsJumping", true);
        }
    }

    void HandleAttack()
    {
        
        // Nhấn chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            moveSpeed = 0;
            // Nếu đang attack rồi thì bỏ qua
            if (animator.GetBool("IsAttacking")) return;

            // Tấn công được phép thực hiện khi đang nhảy hoặc chạy
            animator.SetTrigger("Attack");
            animator.SetBool("IsAttacking", true);
            animator.SetBool("IsRunning", false);
            
        }
    }
    
    void EndAttack()
    {
        animator.SetBool("IsAttacking", false);
        moveSpeed = orginalMoveSpeed;
    }

    void Flip()
    {
        if(movement < 0 && facingRight)
        {
            transform.eulerAngles = new Vector3(0, -180, 0);
            facingRight = false;
        }
        else if(movement > 0 && !facingRight)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            facingRight = true;
        }
        

    }

    void Jump()
    {
        if (isGrounded)
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.y = jumpHeight;
            rb.linearVelocity = velocity;
            isGrounded = false;
            animator.SetBool("Jump", true);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (playerHealth <= 0)
        {
            return;
        }
        playerHealth -= damageAmount;
        animator.SetTrigger("takeDamage");
        CameraShake.instance.Shake(2.5f, 0.15f);
        animator.SetBool("IsAttacking", false);
        ResetState();
    }

    void Die()
    {
        animator.SetBool("Dead", true);
        this.enabled = false;

        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        boxCollider2D.enabled = false;

        CameraShake.instance.Shake(4f, 0.18f);
        Destroy(this.gameObject, 5f);
        StartCoroutine(LoadGameOverAfterDelay(4f)); 
        
       
    }
    
    private IEnumerator LoadGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManagement.instance.LoadLevel("GameOver");
    }
    
    void ResetState()
    {
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsRunning", false);
        moveSpeed = orginalMoveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("OneWayPlatform") )
        {
            animator.SetBool("Jump", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Heart"))
        {
            playerHealth += 1;
            curentHeartsText.text = playerHealth.ToString();
            GameObject tempCollectEffect = Instantiate(collectEffectPrefab, other.transform.position, Quaternion.identity);
            Destroy(tempCollectEffect, .401f);
            Destroy(other.gameObject);
        }
        if (other.CompareTag("EnemyArrow") && playerHealth > 0)  // Only handle arrow collision if player is alive
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Suriken"))
        {
            TakeDamage(1);
        }

        if (other.CompareTag("Diamond"))
        {
            currentDiamonds += 1;
            currentDiamondsText.text = currentDiamonds.ToString();
            GameObject tempCollectEffect = Instantiate(collectEffectPrefab, other.transform.position, Quaternion.identity);
            Destroy(tempCollectEffect, .401f);
            Destroy(other.gameObject);
            
        }

        if (other.CompareTag("VictoryPoint"))
        {
            // Load next level or show victory screen
            SceneManagement.instance.LoadNextLevel();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Apply damage continuously while staying in contact with a Suriken,
        // but only once per contactDamageInterval seconds.
        if (other.CompareTag("Suriken") && playerHealth > 0)
        {
            if (Time.time - lastContactDamageTime >= contactDamageInterval)
            {
                TakeDamage(1);
                lastContactDamageTime = Time.time;
            }
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;

        float dashDirection = facingRight ? 1 : -1;
        // rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashForce * dashDirection, rb.linearVelocity.y);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        // rb.gravityScale = 3f;
    }

    private void OnDrawGizmosSelected()
    {
        if(groundCheckPoint == null)
        {
            return;
        }
        Gizmos.color = Color.red;    
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}
