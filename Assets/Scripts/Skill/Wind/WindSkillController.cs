using UnityEngine;
using Unity.Netcode;

public class WindSkillController : NetworkBehaviour
{
    [Header("Wind Skill Prefabs")]
    public GameObject windSlashPrefab;
    public GameObject galeStepEffectPrefab;
    public GameObject hurricanePrefab;
    public GameObject stormWindPrefab;
    public GameObject lightningStrikePrefab;
    public Transform castPoint;

    [Header("Cooldown Settings")]
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
            Vector2 dir = playerController.IsFacingRight ? Vector2.right : Vector2.left;
            WindSlashServerRpc(castPoint.position, dir);
            qTimer = qCooldown;
        }

        if (Input.GetKeyDown(KeyCode.E) && eTimer <= 0)
        {
            PerformGaleStep();
            eTimer = eCooldown;
        }

        if (Input.GetKeyDown(KeyCode.R) && rTimer <= 0)
        {
            SpawnHurricaneServerRpc(transform.position);
            rTimer = rCooldown;
        }

        if (Input.GetKeyDown(KeyCode.F) && fTimer <= 0)
        {
            CastStormFuryServerRpc();
            fTimer = fCooldown;
        }
    }

    [ServerRpc]
    void WindSlashServerRpc(Vector3 position, Vector2 direction)
    {
        GameObject slash = Instantiate(windSlashPrefab, position, Quaternion.identity);
        if (slash.TryGetComponent<WindSlash>(out var slashScript))
            slashScript.SetDirection(direction);

        if (slash.TryGetComponent<NetworkObject>(out var netObj))
            netObj.Spawn();
    }

    void PerformGaleStep()
    {
        if (playerController == null) return;

        Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
        if (rb != null && playerController.IsOwner)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, playerController.jumpForce);
            if (galeStepEffectPrefab != null)
            {
                GameObject effect = Instantiate(galeStepEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
    }

    [ServerRpc]
    void SpawnHurricaneServerRpc(Vector3 position)
    {
        GameObject hurricane = Instantiate(hurricanePrefab, position, Quaternion.identity);
        hurricane.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    void CastStormFuryServerRpc()
    {
        // ลมพายุ
        Vector3 windStart = new Vector3(-15f, 0f, 0f);
        GameObject wind = Instantiate(stormWindPrefab, windStart, Quaternion.identity);
        wind.GetComponent<NetworkObject>().Spawn();

        // สายฟ้าสุ่ม 5 ตำแหน่ง
        for (int i = 0; i < 5; i++)
        {
            Vector3 lightningPos = new Vector3(Random.Range(-10f, 10f), -3.5f, 0f);
            GameObject lightning = Instantiate(lightningStrikePrefab, lightningPos, Quaternion.identity);
            lightning.GetComponent<NetworkObject>().Spawn();
        }
    }
}
