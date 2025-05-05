using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Services.Analytics;

public class PlayerController : NetworkBehaviour
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
    private Vector2 dashDirection;
    private bool canDash = true;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 20;
    public LayerMask enemyLayers;
    private bool isAttacking = false;
    public float attackCooldown = 0.5f;
    private float lastAttackTime;

    private Animator animator;
    private int dashCount = 0;

    // Animation Sync
    public NetworkVariable<bool> isRunningNet = new(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isJumpingUpNet = new(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isFallingNet = new(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isGroundedNet = new(writePerm: NetworkVariableWritePermission.Owner);

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Update only by owner
        if (IsOwner)
        {
            // ตรวจสอบการอยู่บนพื้น
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (isGrounded) jumpCount = 0;

            if (!isDashing)
            {
                float moveInput = Input.GetAxis("Horizontal");
                rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

                if (moveInput > 0 && !isFacingRight) Flip();
                else if (moveInput < 0 && isFacingRight) Flip();

                if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    jumpCount++;
                }

                if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
                {
                    StartDash();
                }
            }

            if (isDashing)
            {
                rb.linearVelocity = dashDirection * dashSpeed;
                dashTimeCounter -= Time.deltaTime;
                if (dashTimeCounter <= 0 || HitWall())
                {
                    StopDash();
                }
            }

            if (Input.GetMouseButtonDown(0) && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
            {
                isAttacking = true;
                lastAttackTime = Time.time;
                Attack();
                Invoke(nameof(ResetAttack), attackCooldown);
            }

            // Update Animation State (Network Variables)
            isRunningNet.Value = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
            isJumpingUpNet.Value = !isGrounded && rb.linearVelocity.y > 0.1f;
            isFallingNet.Value = !isGrounded && rb.linearVelocity.y < -0.1f;
            isGroundedNet.Value = isGrounded;
        }

        // Animator update (runs on all clients)
        animator.SetBool("isRunning", isRunningNet.Value);
        animator.SetBool("isJumpingUp", isJumpingUpNet.Value);
        animator.SetBool("isFalling", isFallingNet.Value);
        animator.SetBool("isGrounded", isGroundedNet.Value);
    }

    void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimeCounter = dashTime;
        dashCooldownTimer = Time.time + dashCooldown;
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (horizontalInput == 0) horizontalInput = isFacingRight ? 1 : -1;
        dashDirection = new Vector2(horizontalInput, 0).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        // ✅ Count Dash
        dashCount++;

        // ✅ Send Analytics
        AnalyticsService.Instance.RecordEvent(new CustomEvent("player_dash")
    {
        { "client_id", NetworkManager.Singleton.LocalClientId.ToString() },
        { "dash_count", dashCount }
    });

        Debug.Log($"📊 Dash count: {dashCount} by client {NetworkManager.Singleton.LocalClientId}");
    }

    void StopDash()
    {
        isDashing = false;
        rb.gravityScale = 1;
        rb.linearVelocity = Vector2.zero;
        Invoke(nameof(ResetDash), dashCooldown);
    }

    void ResetDash()
    {
        canDash = true;
    }

    bool HitWall()
    {
        float rayDistance = 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDirection, rayDistance, groundLayer);
        return hit.collider != null;
    }

    void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        HashSet<NetworkObject> alreadyHit = new HashSet<NetworkObject>();

        foreach (Collider2D enemy in hitEnemies)
        {
            NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();

            if (enemyNetObj != null && !alreadyHit.Contains(enemyNetObj))
            {
                DealDamageServerRpc(enemyNetObj, attackDamage);
                Debug.Log($"ส่งคำสั่งโจมตี {enemy.gameObject.name} ด้วยดาเมจ {attackDamage}");

                alreadyHit.Add(enemyNetObj); // ✅ บันทึกว่าเคยตีตัวนี้แล้ว
            }
        }
    }



    [ServerRpc(RequireOwnership = false)]
    private void DealDamageServerRpc(NetworkObjectReference enemyRef, int damage)
    {
        if (enemyRef.TryGet(out NetworkObject enemyObj))
        {
            Health health = enemyObj.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
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
