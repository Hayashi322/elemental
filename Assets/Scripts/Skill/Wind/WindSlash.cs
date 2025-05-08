using Unity.Netcode;
using UnityEngine;

public class WindSlash : NetworkBehaviour
{
    public float speed = 12f;
    public int damage = 7; // ✅ เพิ่มตัวแปรดาเมจ
    public GameObject slashEffect;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }

        // Flip sprite ตามทิศทาง
        if (direction.x < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else
            transform.rotation = Quaternion.identity;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);
        }

        if (slashEffect != null)
        {
            Instantiate(slashEffect, transform.position, Quaternion.identity);
        }

        if (TryGetComponent<NetworkObject>(out var netObj))
        {
            netObj.Despawn(true); // ✅ ถูกต้อง: ให้ Server เรียก Despawn
        }
    }
}
