using Unity.Netcode;
using UnityEngine;

public class TidalWave : NetworkBehaviour
{
    public float speed = 5f;
    private Vector2 moveDirection;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        // ✅ เคลื่อนที่
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }

        // ✅ แก้การหัน: ใช้ localScale.x แทน rotation เพื่อความถูกต้องใน 2D
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (direction.x < 0 ? 1 : -1);
        transform.localScale = scale;
    }

    private void FixedUpdate()
    {
        if (!IsServer || rb == null) return;

        rb.linearVelocity = moveDirection * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            var rbTarget = other.GetComponent<Rigidbody2D>();
            if (rbTarget != null)
            {
                rbTarget.AddForce(moveDirection * 5f, ForceMode2D.Impulse);
            }
        }
    }
}
