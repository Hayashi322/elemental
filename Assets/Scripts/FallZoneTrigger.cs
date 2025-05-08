using Unity.Netcode;
using UnityEngine;

public class FallZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                if (!NetworkManager.Singleton.IsServer) return;
                health.TakeDamage(100);
                Debug.Log("🕳️ Player fell off the map! -100 HP");
            }
        }
    }
}
