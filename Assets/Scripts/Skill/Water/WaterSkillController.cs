using Unity.Netcode;
using UnityEngine;

public class WaterSkillController : NetworkBehaviour
{
    [Header("Water Skill Prefabs")]
    public GameObject waterShotPrefab;
    public GameObject aquaShieldPrefab;
    public GameObject tidalWavePrefab;
    public GameObject oceanBlessingPrefab;
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
            WaterShotServerRpc(castPoint.position, dir);
            qTimer = qCooldown;
        }

        if (Input.GetKeyDown(KeyCode.E) && eTimer <= 0)
        {
            SpawnAquaShieldServerRpc(OwnerClientId);
            eTimer = eCooldown;
        }

        if (Input.GetKeyDown(KeyCode.R) && rTimer <= 0)
        {
            Vector2 dir = playerController.IsFacingRight ? Vector2.right : Vector2.left;
            SpawnTidalWaveServerRpc(transform.position, dir);
            rTimer = rCooldown;
        }

        if (Input.GetKeyDown(KeyCode.F) && fTimer <= 0)
        {
            SpawnOceanBlessingServerRpc(transform.position);
            fTimer = fCooldown;
        }
    }

    [ServerRpc]
    void WaterShotServerRpc(Vector3 position, Vector2 direction)
    {
        GameObject waterShot = Instantiate(waterShotPrefab, position, Quaternion.identity);

        if (waterShot.TryGetComponent<WaterShot>(out var shot))
        {
            shot.SetDirection(direction, OwnerClientId);
            shot.damage = 8; // ✅ ดาเมจของ Q
        }

        if (waterShot.TryGetComponent<NetworkObject>(out var netObj))
            netObj.Spawn();
    }

    [ServerRpc]
    void SpawnAquaShieldServerRpc(ulong clientId)
    {
        GameObject shield = Instantiate(aquaShieldPrefab, transform.position, Quaternion.identity);
        if (shield.TryGetComponent<NetworkObject>(out var netObj))
        {
            netObj.SpawnWithOwnership(clientId);
        }

        if (shield.TryGetComponent<WaterShield>(out var shieldScript))
        {
            shieldScript.Init(transform);
        }
    }

    [ServerRpc]
    void SpawnTidalWaveServerRpc(Vector3 position, Vector2 direction)
    {
        GameObject wave = Instantiate(tidalWavePrefab, position, Quaternion.identity);

        if (wave.TryGetComponent<TidalWave>(out var waveScript))
        {
            waveScript.SetDirection(direction);
            waveScript.damage = 6; // ✅ ดาเมจของ R
        }

        if (wave.TryGetComponent<NetworkObject>(out var netObj))
            netObj.Spawn();
    }

    [ServerRpc]
    void SpawnOceanBlessingServerRpc(Vector3 pos)
    {
        GameObject blessing = Instantiate(oceanBlessingPrefab, pos, Quaternion.identity);

        if (blessing.TryGetComponent<OceanBlessing>(out var ocean))
        {
            ocean.healAmount = 5; // ✅ Heal ของ F
        }

        blessing.GetComponent<NetworkObject>().Spawn();
        blessing.transform.SetParent(transform);
    }
}
