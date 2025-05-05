using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(ParticleSystem))]
public class CurveVelocity2D_Rigidbody : MonoBehaviour
{
    public AnimationCurve velocityCurve = AnimationCurve.Linear(0, 0, 1, 5);
    public Vector2 moveDirection = Vector2.right;

    private float duration;
    private float timer;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Get start lifetime from particle system
        ParticleSystem ps = GetComponent<ParticleSystem>();
        duration = ps.main.startLifetime.constant; // Assumes constant start lifetime
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer >= duration)
        {
            Destroy(gameObject);
            return;
        }

        float t = Mathf.Clamp01(timer / duration);
        float speed = velocityCurve.Evaluate(t);
        rb.linearVelocity = moveDirection.normalized * speed;
    }
}