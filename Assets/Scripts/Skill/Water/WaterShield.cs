using Unity.Netcode;
using UnityEngine;

public class WaterShield : NetworkBehaviour
{
    private Transform ownerTransform;

    public void Init(Transform owner)
    {
        ownerTransform = owner;
    }

    public override void OnNetworkSpawn()
    {
        // fallback ถ้ายังไม่มีการกำหนด owner
        if (ownerTransform == null && transform.parent != null)
        {
            ownerTransform = transform.parent;
        }

        Invoke(nameof(DestroyShield), 5f);
    }

    void Update()
    {
        if (ownerTransform != null)
        {
            transform.position = ownerTransform.position;
        }
    }

    void DestroyShield()
    {
        if (IsServer)
        {
            NetworkObject.Despawn(true);
        }
    }
}
