using Unity.Netcode;
using UnityEngine;

public class FireBall : NetworkBehaviour
{
    public float speed = 8f;
    public GameObject explosionEffect;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    public int damage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        // ✅ ให้ Rigidbody เคลื่อนที่
        rb.linearVelocity = moveDirection * speed;

        // ✅ หมุนพาร์ติเคิล (โดยกลับ localScale.x)
        if (direction.x < 0)
            transform.localScale = new Vector3(-1f, 1f, 1f);  // หันซ้าย
        else
            transform.localScale = new Vector3(1f, 1f, 1f);   // หันขวา
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall"))
        {
            // ✅ สร้างเอฟเฟกต์ระเบิด
            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            // ✅ ทำลายลูกไฟ (เฉพาะฝั่ง Server เพราะใช้ Netcode)
            NetworkObject.Despawn(true);
        }
    }
}
