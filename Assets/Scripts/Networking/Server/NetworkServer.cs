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

    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.ConnectionApprovalCallback = ApprovalCheck;
        networkManager.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        Debug.Log("✅ Server started. Registering callbacks.");

        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        networkManager.OnClientConnectedCallback += OnClientConnected;

        // ✅ ใช้ delegate signature ใหม่ของ Netcode 2.3.0
        //networkManager.SceneManager.OnLoadComplete -= OnSceneLoadCompleted;
        //networkManager.SceneManager.OnLoadComplete += OnSceneLoadCompleted;

    }

  

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"✅ Client connected: {clientId}");

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("🧭 Host client connected — loading CharacterSelect scene...");
            NetworkManager.Singleton.SceneManager.LoadScene("CharacterSelect", LoadSceneMode.Single);
        }
    }

    // ✅ delegate แบบใหม่ที่ถูกต้องใน Netcode 2.3.0
    private void OnSceneLoadCompleted(
    string sceneName,
    LoadSceneMode loadSceneMode,
    List<ulong> clientsCompleted,
    List<ulong> clientsTimedOut)
    {
        if (sceneName != "Lv.1") return;

        Debug.Log($"🎯 Scene {sceneName} loaded → spawning players...");

        foreach (ulong clientId in clientsCompleted)
        {
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





    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"❎ Client disconnected: {clientId}");

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

        Debug.Log($"🔓 Approving connection for {userData.userName} ({userData.characterName})");

        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        authIdToUserData[userData.userAuthId] = userData;

        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Rotation = Quaternion.identity;
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

        Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-3f, 3f), 1f, 0);
        GameObject player = GameObject.Instantiate(prefab, spawnPos, Quaternion.identity);

        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("❌ No NetworkObject on the spawned player prefab!");
            return;
        }

        netObj.SpawnAsPlayerObject(clientId);
        Debug.Log($"✅ Player spawned for {clientId} as {characterName}");
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

    public void Dispose()
    {
        Debug.Log("🧹 Disposing NetworkServer...");

        if (networkManager == null) return;

        networkManager.ConnectionApprovalCallback = null;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnServerStarted -= OnServerStarted;

        //networkManager.SceneManager.OnLoadComplete -= OnSceneLoadCompleted;

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }

    
}
