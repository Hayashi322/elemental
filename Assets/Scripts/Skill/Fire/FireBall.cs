using Unity.Netcode;
using UnityEngine;

public class FireBall : NetworkBehaviour
{
    public float speed = 8f;
    public GameObject explosionEffect;
    public int damage = 10; // ✅ ดาเมจของ FireBall

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private ulong ownerClientId;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 direction, ulong ownerId)
    {
        moveDirection = direction.normalized;
        ownerClientId = ownerId;

        // เคลื่อนที่
        if (rb != null)
            rb.linearVelocity = moveDirection * speed;

        // หมุน sprite/prefab ไปตามทิศ
        transform.localScale = new Vector3(direction.x < 0 ? -1f : 1f, 1f, 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        // โดน Player คนอื่น
        if (collision.gameObject.CompareTag("Player"))
        {
            var netObj = collision.gameObject.GetComponent<NetworkObject>();
            if (netObj != null && netObj.OwnerClientId != ownerClientId)
            {
                var health = collision.gameObject.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
            }
        }

        // ทำลายเมื่อชนอะไรก็ตาม
        if (collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Wall") ||
            collision.gameObject.CompareTag("Player"))
        {
            if (explosionEffect != null)
                Instantiate(explosionEffect, transform.position, Quaternion.identity);

            if (TryGetComponent<NetworkObject>(out var netObj))
                netObj.Despawn(true);
        }
    }
}
