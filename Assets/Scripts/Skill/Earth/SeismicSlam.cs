using UnityEngine;
using Unity.Netcode;

public class SeismicSlam : NetworkBehaviour
{
    public float radius = 3f;
    public float force = 7f;
    public int damage = 6;
    public LayerMask enemyLayer;

    public ulong ownerId; // ✅ สำหรับกันตีตัวเอง

    private void Start()
    {
        if (!IsServer) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Player"))
            {
                NetworkObject netObj = enemy.GetComponent<NetworkObject>();
                if (netObj != null && netObj.OwnerClientId != ownerId)
                {
                    // ✅ ทำดาเมจ
                    Health health = enemy.GetComponent<Health>();
                    if (health != null)
                    {
                        health.TakeDamage(damage);
                    }

                    // ✅ ผลักออกไปจากจุดศูนย์กลาง
                    Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 pushDir = (enemy.transform.position - transform.position).normalized;
                        rb.AddForce(pushDir * force, ForceMode2D.Impulse);
                    }
                }
            }
        }

        GetComponent<NetworkObject>().Despawn(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
