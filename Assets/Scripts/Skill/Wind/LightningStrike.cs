using Unity.Netcode;
using UnityEngine;

public class LightningStrike : NetworkBehaviour
{
    public float lifeTime = 1.0f;

    private void Start()
    {
        if (IsServer)
        {
            Invoke(nameof(DestroySelf), lifeTime);
        }
    }

    void DestroySelf()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(12); // ฟ้าผ่าดาเมจ
        }
    }
}
