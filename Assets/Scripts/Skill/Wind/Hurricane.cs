using Unity.Netcode;
using UnityEngine;

public class Hurricane : NetworkBehaviour
{
    public float duration = 5f;
    public float pullForce = 10f;
    public float pullRadius = 5f;
    public int tickDamage = 1;
    public float damageInterval = 0.5f;

    private float timer;
    private float damageTimer;
    private ulong ownerClientId;

    public void SetOwner(ulong clientId)
    {
        ownerClientId = clientId;
    }

    void Start()
    {
        timer = duration;
        damageTimer = 0f;
    }

    void Update()
    {
        if (!IsServer) return;

        timer -= Time.deltaTime;
        damageTimer -= Time.deltaTime;

        if (timer <= 0f)
        {
            GetComponent<NetworkObject>().Despawn();
            return;
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pullRadius);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Player") && col.TryGetComponent<NetworkObject>(out var netObj) && netObj.OwnerClientId != ownerClientId)
            {
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = (transform.position - col.transform.position).normalized;
                    rb.AddForce(dir * pullForce * Time.deltaTime, ForceMode2D.Force);
                }

                // ทำดาเมจทุก 0.5 วินาที
                if (damageTimer <= 0f && col.TryGetComponent<Health>(out var health))
                {
                    health.TakeDamage(tickDamage);
                }
            }
        }

        if (damageTimer <= 0f)
        {
            damageTimer = damageInterval;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}
