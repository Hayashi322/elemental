using UnityEngine;
using Unity.Netcode;

public class GaleStep : NetworkBehaviour
{
    public float duration = 5f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        var controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.EnableTripleJump(duration);
        }

        Destroy(gameObject, duration); // ลบตัวเองหลังหมดเวลา
    }
}
