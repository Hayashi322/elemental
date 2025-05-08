using UnityEngine;
using Unity.Netcode;

public class EarthSkillController : NetworkBehaviour
{
    public GameObject rockThrowPrefab;
    public GameObject earthWallPrefab;
    public GameObject seismicSlamPrefab;
    public GameObject terraShockwavePrefab;
    public Transform castPoint;

    public float qCooldown = 2f;
    public float eCooldown = 5f;
    public float rCooldown = 8f;
    public float fCooldown = 12f;

    private float qTimer, eTimer, rTimer, fTimer;
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
            RockThrowServerRpc(castPoint.position, dir);
            qTimer = qCooldown;
        }

        if (Input.GetKeyDown(KeyCode.E) && eTimer <= 0)
        {
            SpawnEarthenWallServerRpc(transform.position);
            eTimer = eCooldown;
        }

        if (Input.GetKeyDown(KeyCode.R) && rTimer <= 0)
        {
            SpawnSeismicSlamServerRpc(transform.position);
            rTimer = rCooldown;
        }

        if (Input.GetKeyDown(KeyCode.F) && fTimer <= 0)
        {
            Vector2 dir = playerController.IsFacingRight ? Vector2.right : Vector2.left;
            SpawnTerraShockwaveServerRpc(transform.position, dir);
            fTimer = fCooldown;
        }
    }

    [ServerRpc]
    void RockThrowServerRpc(Vector3 position, Vector2 direction)
    {
        GameObject rock = Instantiate(rockThrowPrefab, position, Quaternion.identity);
        if (rock.TryGetComponent<RockThrow>(out var rockScript))
            rockScript.SetDirection(direction);

        rock.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    void SpawnEarthenWallServerRpc(Vector3 position)
    {
        GameObject wall = Instantiate(earthWallPrefab, position, Quaternion.identity);
        wall.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    void SpawnSeismicSlamServerRpc(Vector3 position)
    {
        GameObject slam = Instantiate(seismicSlamPrefab, position, Quaternion.identity);
        slam.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    void SpawnTerraShockwaveServerRpc(Vector3 position, Vector2 direction)
    {
        GameObject shockwave = Instantiate(terraShockwavePrefab, position, Quaternion.identity);

        if (shockwave.TryGetComponent<TerraShockwave>(out var shockScript))
            shockScript.SetDirection(direction, OwnerClientId); // ✅ ป้องกันตีตัวเอง

        shockwave.GetComponent<NetworkObject>().Spawn();
    }
}
