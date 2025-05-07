using Unity.Netcode;
using UnityEngine;

public class Hurricane : NetworkBehaviour
{
    public float duration = 5f;
    public float pullForce = 10f;
    public float pullRadius = 5f;

    private float timer;

    void Start()
    {
        timer = duration;
    }

    void Update()
    {
        if (!IsServer) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            GetComponent<NetworkObject>().Despawn();
            return;
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pullRadius);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Player") && col.gameObject != gameObject)
            {
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = (transform.position - col.transform.position).normalized;
                    rb.AddForce(dir * pullForce * Time.deltaTime, ForceMode2D.Force);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}
