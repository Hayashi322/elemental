using UnityEngine;
using Unity.Netcode;

public class InfernoWave : NetworkBehaviour
{
    public int damage = 3;
    public LayerMask hitLayers;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (((1 << other.gameObject.layer) & hitLayers) != 0)
        {
            if (other.CompareTag("Player") && other.TryGetComponent<Health>(out var hp))
            {
                hp.TakeDamage(damage);
                Debug.Log($"🔥 InfernoWave hit {other.name} for {damage} damage");
            }
        }
    }
}