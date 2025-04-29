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

        // 🔁 Restore mapping **ทันที**
        Server.RestoreMappings(CachedClientIdToAuth, CachedAuthIdToUserData);
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