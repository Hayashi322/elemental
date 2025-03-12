using UnityEngine;
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;
    private int jumpCount = 0;
    public int maxJumpCount = 2;
    private Rigidbody2D rb;
    private bool isFacingRight = true;
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f;
    private float dashTimeCounter;
    private float dashCooldownTimer = 0f;
    private bool isDashing;
    private Vector2 dashDirection; // ทิศทางของแดช
    private bool canDash = true;
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 20;
    public LayerMask enemyLayers;
    private bool isAttacking = false;
    public float attackCooldown = 0.5f;
    private float lastAttackTime;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        // ตรวจสอบการอยู่บนพื้น
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded) jumpCount = 0;
        if (!isDashing)
        {
            // การเคลื่อนที่
            float moveInput = Input.GetAxis("Horizontal");
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            // การพลิกตัวละคร
            if (moveInput > 0 && !isFacingRight) Flip();
            else if (moveInput < 0 && isFacingRight) Flip();
            // การกระโดด
            if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpCount++;
            }
            // การแดช
            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            {
                StartDash();
            }
        }
        // จัดการแดช
        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            dashTimeCounter -= Time.deltaTime;
            if (dashTimeCounter <= 0 || HitWall())
            {
                StopDash();
            }
        }
        // การโจมตี
        if (Input.GetMouseButtonDown(0) && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            Attack();
            Invoke(nameof(ResetAttack), attackCooldown);
        }
    }
    void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimeCounter = dashTime;
        dashCooldownTimer = Time.time + dashCooldown;
        // คำนวณทิศทางแดช
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (horizontalInput == 0) horizontalInput = isFacingRight ? 1 : -1;
        dashDirection = new Vector2(horizontalInput, 0).normalized;
        rb.linearVelocity = Vector2.zero; // รีเซ็ตความเร็วก่อนแดช
        rb.gravityScale = 0; // ปิดแรงโน้มถ่วงขณะแดช
    }
    void StopDash()
    {
        isDashing = false;
        rb.gravityScale = 1; // เปิดแรงโน้มถ่วงคืน
        rb.linearVelocity = Vector2.zero; // หยุดความเร็วแดช
        Invoke(nameof(ResetDash), dashCooldown);
    }
    void ResetDash()
    {
        canDash = true;
    }
    bool HitWall()
    {
        // ตรวจสอบว่าตัวละครชนกำแพงหรือไม่ (ใช้ Raycast)
        float rayDistance = 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDirection, rayDistance, groundLayer);
        return hit.collider != null;
    }
    void Attack()
    {
        /*Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
        }*/
    }
    void ResetAttack()
    {
        isAttacking = false;
    }
    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}