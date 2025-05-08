using Unity.Netcode;
using UnityEngine;

public class OceanBlessing : NetworkBehaviour
{
    public float duration = 3f;
    public int healAmount = 5;

    private void Start()
    {
        if (IsServer)
        {
            // ฟื้นพลังให้เจ้าของ (parent)
            if (transform.parent != null)
            {
                Health health = transform.parent.GetComponent<Health>();
                if (health != null)
                {
                    health.Heal(healAmount);
                    Debug.Log($"💧 OceanBlessing healed {health.name} for {healAmount} HP");
                }
            }

            // ลบตัวเองหลังจาก 3 วินาที
            Invoke(nameof(Despawn), duration);
        }
    }

    void Despawn()
    {
        if (TryGetComponent<NetworkObject>(out var netObj))
        {
            netObj.Despawn(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
