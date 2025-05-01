using System;
using System.Collections;
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
    private List<Transform> spawnPoints = new List<Transform>();
    private int nextSpawnIndex = 0;

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
        if (sceneName == "Lv.1")
        {
            var userData = GetUserDataByClientId(clientId);
            if (userData != null)
            {
                SpawnCustomPlayerObject(clientId, userData.characterName);
            }
            else
            {
                Debug.LogWarning($"⚠️ userData is NULL for client {clientId} in OnSceneLoadCompleted");
            }
        }
    }


    public void SpawnCustomPlayerObject(ulong clientId, string characterName)
    {
        HostSingleton.Instance.StartCoroutine(SpawnDelayed(clientId, characterName));
    }

    private IEnumerator SpawnDelayed(ulong clientId, string characterName)
    {
        // 🔁 Despawn เดิม (ถ้ามี)
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
            {
                Debug.Log($"🧹 Despawning old PlayerObject for client {clientId}");

                var oldPlayerObject = client.PlayerObject;
                oldPlayerObject.Despawn(true);

                NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject = null;
            }
        }

        yield return null; // ✅ รอให้ Netcode เคลียร์ก่อน

        GameObject prefab = GetCharacterPrefab(characterName);
        if (prefab == null)
        {
            Debug.LogError($"❌ Character prefab for '{characterName}' not found!");
            yield break;
        }

        Vector3 spawnPos = GetNextSpawnPosition();
        GameObject player = GameObject.Instantiate(prefab, spawnPos, Quaternion.identity);

        // ✅ Reset health ก่อน spawn
        Health health = player.GetComponent<Health>();
        if (health != null)
        {
            health.ResetHealth();
            Debug.Log($"🎯 Reset health for client {clientId} to {health.CurrentHealth.Value}");
        }

        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("❌ No NetworkObject on the spawned prefab!");
            yield break;
        }

        netObj.SpawnAsPlayerObject(clientId);
        Debug.Log($"✅ Spawned new PlayerObject for client {clientId} as {characterName} at {spawnPos}");

        // ✅ ป้องกันการ spawn เงียบ (ซ้ำ)
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var connectedClient))
        {
            if (connectedClient.PlayerObject == null || !connectedClient.PlayerObject.IsSpawned)
            {
                connectedClient.PlayerObject = netObj;
                Debug.Log($"🛠 Assigned new PlayerObject manually for client {clientId}");
            }
        }
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

        // บันทึก mapping ทันที
        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        authIdToUserData[userData.userAuthId] = userData;

        // แคชไว้ใน HostSingleton ด้วย (ถ้ายังไม่ได้ทำ)
        HostSingleton.Instance.CachedClientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        HostSingleton.Instance.CachedAuthIdToUserData[userData.userAuthId] = userData;

        response.Approved = true;
        response.CreatePlayerObject = false;
        Debug.Log($"📌 Approval: clientId={request.ClientNetworkId}, userAuthId={userData.userAuthId}, character={userData.characterName}");

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
            else
            {
                Debug.LogError($"❌ authId '{authId}' not found in authIdToUserData");
            }
        }
        else
        {
            Debug.LogError($"❌ clientId '{clientId}' not found in clientIdToAuth");
        }

        Debug.LogError($"❌ userData is NULL for client {clientId} in GetUserDataByClientId");
        Debug.LogError($"🔍 Current clientIdToAuth keys: {string.Join(", ", clientIdToAuth.Keys)}");
        Debug.LogError($"🔍 Current authIdToUserData keys: {string.Join(", ", authIdToUserData.Keys)}");
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

    private Vector3 GetNextSpawnPosition()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("⚠️ No spawn points available! Using (0,0,0).");
            return Vector3.zero;
        }

        Vector3 pos = spawnPoints[nextSpawnIndex].position;
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Count;
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

    public Dictionary<ulong, string> GetClientIdToAuth() => clientIdToAuth;
    public Dictionary<string, UserData> GetAuthIdToUserData() => authIdToUserData;

    public void RestoreMappings(Dictionary<ulong, string> cidToAuth, Dictionary<string, UserData> authToUser)
    {
        clientIdToAuth = new Dictionary<ulong, string>(cidToAuth);
        authIdToUserData = new Dictionary<string, UserData>(authToUser);

        Debug.Log($"♻️ Restored mappings.");
        Debug.Log($"🔁 clientIdToAuth keys: {string.Join(", ", clientIdToAuth.Keys)}");
        Debug.Log($"🔁 authIdToUserData keys: {string.Join(", ", authIdToUserData.Keys)}");
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