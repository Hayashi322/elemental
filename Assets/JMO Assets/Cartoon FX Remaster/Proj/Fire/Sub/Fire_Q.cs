using Unity.Netcode;
using UnityEngine;

public class Fire_Q : NetworkBehaviour
{
    public GameObject fireBallPrefab;
    public Transform firePoint;
    public float fireSpeed = 8f;
    private Vector2 moveDirection;

    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ShootServerRpc(playerController.IsFacingRight);
        }
    }
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        GetComponent<Rigidbody2D>().linearVelocity = moveDirection * fireSpeed;
    }

    [ServerRpc]
    void ShootServerRpc(bool isFacingRight)
    {
        GameObject fireball = Instantiate(fireBallPrefab, firePoint.position, Quaternion.identity);
        fireball.GetComponent<NetworkObject>().Spawn();

        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        fireball.GetComponent<Rigidbody2D>().linearVelocity = direction * fireSpeed;
        fireball.transform.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1);
    }
}
