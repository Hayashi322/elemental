using UnityEngine;
using Unity.Netcode;

public class TerraShockwave : NetworkBehaviour
{
    public float speed = 5f;
    public int damage = 10;
    public LayerMask hitLayers;

    private Vector2 moveDirection;
    private Rigidbody2D rb;
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
            rb.linearVelocity = moveDirection * speed;

        // ปรับการหัน sprite
        transform.localScale = new Vector3(direction.x < 0 ? -1f : 1f, 1f, 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (((1 << other.gameObject.layer) & hitLayers) != 0)
        {
            if (other.CompareTag("Player"))
            {
                NetworkObject netObj = other.GetComponent<NetworkObject>();
                if (netObj != null && netObj.OwnerClientId != ownerClientId)
                {
                    Health hp = other.GetComponent<Health>();
                    if (hp != null)
                        hp.TakeDamage(damage);
                }
            }

            GetComponent<NetworkObject>().Despawn();
        }
    }
}
