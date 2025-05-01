using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    public static HostSingleton Instance { get; private set; }

    public HostGameManager GameManager { get; private set; }
    public CharacterPrefabLibrary prefabLibrary;
    public NetworkServer Server { get; private set; }

    public Dictionary<ulong, string> CachedClientIdToAuth = new();
    public Dictionary<string, UserData> CachedAuthIdToUserData = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ✅ เพิ่มตัวจับเวลาให้กับ HostSingleton
            if (GetComponent<ClientSceneTimer>() == null)
            {
                gameObject.AddComponent<ClientSceneTimer>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForNetworkManagerAndCreateServer());
    }

    private IEnumerator WaitForNetworkManagerAndCreateServer()
    {
        while (NetworkManager.Singleton == null)
            yield return null;

        Server = new NetworkServer(NetworkManager.Singleton);

        yield return null; // ✅ รอ 1 เฟรมก่อน Restore

        // 🔁 ต้องแน่ใจว่าเรียกทันทีหลังสร้าง Server
        Server.RestoreMappings(CachedClientIdToAuth, CachedAuthIdToUserData);

        Debug.Log("✅ Server created and mappings restored.");
    }

    public void CreateHost()
    {
        if (GameManager != null)
        {
            Debug.Log("⚠️ HostGameManager already created");
            return;
        }

        GameManager = new HostGameManager(prefabLibrary);
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }

    

}