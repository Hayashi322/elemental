using UnityEngine;
using Unity.Netcode;

public class RockThrow : NetworkBehaviour
{
    public float speed = 8f;
    private Vector2 moveDirection;
    private Rigidbody2D rb;

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

        // Flip ก้อนหินหากหันซ้าย
        if (direction.x < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else
            transform.rotation = Quaternion.identity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            var netObj = collision.gameObject.GetComponent<NetworkObject>();
            if (netObj != null && netObj.OwnerClientId != OwnerClientId)
            {
                var health = collision.gameObject.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(10); // ✅ ดาเมจที่ต้องการ
                }
            }
        }

        // ลบการ Instantiate explosionEffect
        // ไม่ต้องทำอะไร แค่ลบตัวเอง

        GetComponent<NetworkObject>().Despawn();
    }
}
