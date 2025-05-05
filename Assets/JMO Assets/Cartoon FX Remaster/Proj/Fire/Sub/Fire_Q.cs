using UnityEngine;

public class Fire_Q : MonoBehaviour
{
    public float speed = 10f;
    public GameObject explosionEffect;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed; // Fireball moves in the direction it's facing
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Spawn explosion effect and destroy the fireball
        Instantiate(explosionEffect, transform.position, transform.rotation);
        Destroy(gameObject);  // Destroy fireball after collision
    }
}