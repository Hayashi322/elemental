using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;

    public GameObject earthPrefab;
    public GameObject firePrefab;
    public GameObject waterPrefab;
    public GameObject windPrefab;

    private Dictionary<ulong, string> clientIdToAuth = new();
    private Dictionary<string, UserData> authIdToUserData = new();
    private List<Vector3> availableSpawnPoints = new();
    private List<Transform> spawnPoints = new List<Transform>();

    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.ConnectionApprovalCallback = ApprovalCheck;
        networkManager.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        Debug.Log("✅ Server started. Registering Callbacks.");

        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        networkManager.OnClientConnectedCallback += OnClientConnected;

        networkManager.SceneManager.OnLoadComplete -= OnSceneLoadCompleted;
        networkManager.SceneManager.OnLoadComplete += OnSceneLoadCompleted;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"✅ Client connected: {clientId}");

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("🛁 Host client connected — loading CharacterSelect scene...");
            NetworkManager.Singleton.SceneManager.LoadScene("CharacterSelect", LoadSceneMode.Single);
        }
    }

    private void OnSceneLoadCompleted(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        Debug.Log($"🎯 Scene {sceneName} loaded for client {clientId}");

        if (sceneName == "Lv.1")
        {
            // ✅ เมื่อ Lv.1 โหลดเสร็จ → เตรียม spawn points ทันที
            CacheSpawnPoints();

            var userData = GetUserDataByClientId(clientId);

            if (userData != null)
            {
                SpawnCustomPlayerObject(clientId, userData.characterName);
            }
            else
            {
                Debug.LogWarning($"⚠️ userData for client {clientId} is null. Cannot spawn player.");
            }
        }
    }

    private void InitializeSpawnPoints()
    {
        availableSpawnPoints = new List<Vector3>()
        {
            new Vector3(-7, 2, 0),
            new Vector3(-7, -2, 0),
            new Vector3(7, 3.5f, 0),
            new Vector3(7, -1.5f, 0) 
        };
    }

    private Vector3 GetAndRemoveRandomSpawnPoint()
    {
        int index = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
        Vector3 point = availableSpawnPoints[index];
        availableSpawnPoints.RemoveAt(index);
        return point;
    }

    private void SpawnCustomPlayerObject(ulong clientId, string characterName)
    {
        Debug.Log($"🧩 [Spawn] Requested for {clientId} with {characterName}");

        GameObject prefab = GetCharacterPrefab(characterName);
        if (prefab == null)
        {
            Debug.LogError($"❌ Character prefab for '{characterName}' not found!");
            return;
        }

        // 📌 Cache SpawnPoints เฉพาะครั้งแรก
        if (spawnPoints.Count == 0)
        {
            CacheSpawnPoints();
        }

        Vector3 spawnPos = GetUniqueSpawnPosition();
        GameObject player = GameObject.Instantiate(prefab, spawnPos, Quaternion.identity);

        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("❌ No NetworkObject on the spawned player prefab!");
            return;
        }

        netObj.SpawnAsPlayerObject(clientId);
        Debug.Log($"✅ Player spawned for {clientId} as {characterName} at {spawnPos}");
    }


    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"❌ Client disconnected: {clientId}");

        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            clientIdToAuth.Remove(clientId);
            authIdToUserData.Remove(authId);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        Debug.Log($"🔐 Approving connection for {userData.userName} ({userData.characterName})");

        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        authIdToUserData[userData.userAuthId] = userData;

        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Rotation = Quaternion.identity;
    }

    private GameObject GetCharacterPrefab(string characterName)
    {
        return characterName switch
        {
            "Fire" => firePrefab,
            "Earth" => earthPrefab,
            "Wind" => windPrefab,
            _ => waterPrefab
        };
    }

    public UserData GetUserDataByClientId(ulong clientId)
    {
        if (clientIdToAuth.TryGetValue(clientId, out string authId))
        {
            if (authIdToUserData.TryGetValue(authId, out UserData data))
            {
                return data;
            }
        }
        return null;
    }

    private void CacheSpawnPoints()
    {
        spawnPoints.Clear();
        foreach (var obj in GameObject.FindGameObjectsWithTag("SpawnPoint"))
        {
            spawnPoints.Add(obj.transform);
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("⚠️ No spawn points found with tag 'SpawnPoint'!");
        }
        else
        {
            Debug.Log($"📌 Cached {spawnPoints.Count} spawn points.");
        }
    }



    private Vector3 GetUniqueSpawnPosition()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("⚠️ No spawn points available! Using (0,0,0).");
            return Vector3.zero;
        }

        int index = UnityEngine.Random.Range(0, spawnPoints.Count);
        Vector3 pos = spawnPoints[index].position;
        spawnPoints.RemoveAt(index); 
        return pos;
    }

    public bool AreAllPlayersReady()
    {
        foreach (var userData in authIdToUserData.Values)
        {
            if (!userData.isReady)
            {
                return false;
            }
        }
        return true;
    }



    public void Dispose()
    {
        Debug.Log("🧹 Disposing NetworkServer...");

        if (networkManager == null) return;

        networkManager.ConnectionApprovalCallback = null;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnServerStarted -= OnServerStarted;

        networkManager.SceneManager.OnLoadComplete -= OnSceneLoadCompleted;

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}
