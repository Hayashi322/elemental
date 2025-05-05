using UnityEngine;

public class FallingFireball : MonoBehaviour
{
    public float speed = 5f;
    public GameObject explosionEffect;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.down * speed; // Move straight down
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Spawn explosion effect and destroy the fireball
        Instantiate(explosionEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}