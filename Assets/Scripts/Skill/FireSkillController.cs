using Unity.Netcode;
using UnityEngine;

public class FireSkillController : NetworkBehaviour
{
    [Header("Fire Skill Prefabs")]
    public GameObject fireballPrefab;
    public GameObject flameEffectPrefab;
    public GameObject infernoWavePrefab;
    public GameObject meteorPrefab;
    public Transform castPoint;

    [Header("Settings")]
    public float fireballSpeed = 10f;

    [Header("Cooldown Times")]
    public float qCooldown = 2f;
    public float eCooldown = 5f;
    public float rCooldown = 8f;
    public float fCooldown = 12f;

    private float qTimer;
    private float eTimer;
    private float rTimer;
    private float fTimer;

    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!IsOwner || castPoint == null) return;

        qTimer -= Time.deltaTime;
        eTimer -= Time.deltaTime;
        rTimer -= Time.deltaTime;
        fTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && qTimer <= 0)
        {
            Vector3 direction = playerController.IsFacingRight ? Vector2.right : Vector2.left;
            FireballServerRpc(castPoint.position, direction);
            qTimer = qCooldown;
        }

        if (Input.GetKeyDown(KeyCode.E) && eTimer <= 0)
        {
            FlameEffectServerRpc();
            eTimer = eCooldown;
        }

        if (Input.GetKeyDown(KeyCode.R) && rTimer <= 0)
        {
            InfernoWaveServerRpc(transform.position);
            rTimer = rCooldown;
        }

        if (Input.GetKeyDown(KeyCode.F) && fTimer <= 0)
        {
            MeteorRainServerRpc(transform.position + Vector3.up * 8f);
            fTimer = fCooldown;
        }
    }

    [ServerRpc]
    void FireballServerRpc(Vector3 position, Vector3 direction)
    {
        if (fireballPrefab == null) return;

        GameObject fireball = Instantiate(fireballPrefab, position, Quaternion.identity);

        if (fireball.TryGetComponent<FireBall>(out var fireScript))
        {
            fireScript.SetDirection(direction);
            fireScript.damage = 10; // ✅ Q สกิลทำดาเมจ 10
        }

        if (fireball.TryGetComponent<NetworkObject>(out var netObj))
            netObj.Spawn();
    }

    [ServerRpc]
    void FlameEffectServerRpc()
    {
        if (flameEffectPrefab == null) return;

        GameObject flame = Instantiate(flameEffectPrefab, transform.position, Quaternion.identity);

        if (flame.TryGetComponent<FlameEffect>(out var effect))
            effect.Init(transform, OwnerClientId); // 🔁 ให้ตามตัวนี้

        if (flame.TryGetComponent<NetworkObject>(out var netObj))
            netObj.Spawn();
    }


    [ServerRpc]
    void InfernoWaveServerRpc(Vector3 position)
    {
        if (infernoWavePrefab == null) return;

        GameObject wave = Instantiate(infernoWavePrefab, position, Quaternion.identity);
        if (wave.TryGetComponent<NetworkObject>(out var netObj))
            netObj.Spawn();
    }

    [ServerRpc]
    void MeteorRainServerRpc(Vector3 spawnPosition)
    {
        if (meteorPrefab == null) return;

        GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
        if (meteor.TryGetComponent<Rigidbody2D>(out var rb))
            rb.linearVelocity = Vector2.down * 7f;

        if (meteor.TryGetComponent<NetworkObject>(out var netObj))
            netObj.Spawn();
    }
}
