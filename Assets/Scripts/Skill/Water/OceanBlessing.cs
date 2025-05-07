using Unity.Netcode;
using UnityEngine;

public class OceanBlessing : NetworkBehaviour
{
    public float duration = 3f;

    private void Start()
    {
        if (IsServer)
        {
            // ทำลายหลังจาก 3 วินาทีบน Server
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
