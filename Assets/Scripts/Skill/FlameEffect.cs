using Unity.Netcode;
using UnityEngine;

public class FlameEffect : NetworkBehaviour
{
    public int damage = 1;
    private Transform target;
    private float lifetime = 5f;

    public void Init(Transform followTarget, ulong ownerId)
    {
        target = followTarget;
        this.ownerClientId = ownerId;
        Invoke(nameof(DestroySelf), lifetime);
    }

    private ulong ownerClientId;

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<NetworkObject>(out var netObj))
        {
            if (netObj.OwnerClientId != ownerClientId &&
                other.TryGetComponent<Health>(out var health))
            {
                health.TakeDamage(damage);
                Debug.Log($"🔥 FlameEffect hit {netObj.name} for {damage} damage");
            }
        }
    }

    private void DestroySelf()
    {
        if (IsServer && TryGetComponent<NetworkObject>(out var netObj))
        {
            netObj.Despawn(true);
        }
    }
}
