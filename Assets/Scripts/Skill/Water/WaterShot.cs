using UnityEngine;
using Unity.Netcode;

public class WaterShot : NetworkBehaviour
{
    public float speed = 10f;
    public int damage = 8; // ✅ Damage ของ WaterShot
    public GameObject explosionEffect;

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

        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }

        // Flip effect if shooting left
        if (direction.x < 0)
            transform.localScale = new Vector3(-1f, 1f, 1f);
        else
            transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

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

        if (collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Wall") ||
            collision.gameObject.CompareTag("Player"))
        {
            if (explosionEffect != null)
                Instantiate(explosionEffect, transform.position, Quaternion.identity);

            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
