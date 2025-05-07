using Unity.Netcode;
using UnityEngine;

public class StormFury : NetworkBehaviour
{
    public float moveSpeed = 5f;

    private void FixedUpdate()
    {
        if (!IsServer) return;

        transform.position += Vector3.right * moveSpeed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            var rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(Vector2.right * 5f, ForceMode2D.Impulse); // พัดไปทางขวา
            }
        }
    }
}
