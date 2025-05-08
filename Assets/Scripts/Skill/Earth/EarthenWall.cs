using UnityEngine;
using Unity.Netcode;

public class EarthenWall : NetworkBehaviour
{
    public float duration = 5f;

    private void Start()
    {
        // Destroy wall after duration
        Invoke(nameof(DestroyWall), duration);
    }

    void DestroyWall()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
