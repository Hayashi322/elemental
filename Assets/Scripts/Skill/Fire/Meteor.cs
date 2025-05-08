using UnityEngine;
using Unity.Netcode;

public class Meteor : NetworkBehaviour
{
    public float fallSpeed = 7f;
    public int damage = 12;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed;
        rb.linearVelocity = Vector2.down * fallSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player") && other.TryGetComponent<Health>(out var health))
        {
            health.TakeDamage(damage);
        }

        GetComponent<NetworkObject>().Despawn(true);
    }
}
