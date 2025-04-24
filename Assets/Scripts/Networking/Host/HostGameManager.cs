using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class HostGameManager : IDisposable
{
    private Allocation allocation;
    private string joinCode;
    private string lobbyId;

    private NetworkServer networkServer;
    public NetworkServer Server => networkServer;

    private const int MaxConnections = 2;
    private const string GameSceneName = "Lv.1";
    private const string JoinCodeKey = "JoinCode";
    private CharacterPrefabLibrary prefabLibrary;

   

    public HostGameManager(CharacterPrefabLibrary library)
    {
        this.prefabLibrary = library;
    }

    public async Task StartHostAsync()
    {
        // STEP 1: เตรียม Relay Allocation
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            PlayerPrefs.SetString(JoinCodeKey, joinCode);
            Debug.Log("✅ JoinCode: " + joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        // STEP 2: ตั้งค่า Transport
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        // STEP 3: สร้าง Lobby
        try
        {
            var playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
            var lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "JoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MaxConnections, lobbyOptions);
            lobbyId = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            return;
        }

        // STEP 4: Debug ตรวจสอบ PrefabLibrary
        Debug.Log("🔥 prefabLibrary = " + (prefabLibrary == null ? "❌ NULL" : "✅ OK"));

        // STEP 5: สร้าง NetworkServer และส่ง prefab
        networkServer = new NetworkServer(NetworkManager.Singleton);
        networkServer.earthPrefab = prefabLibrary.earthPrefab;
        networkServer.firePrefab = prefabLibrary.firePrefab;
        networkServer.waterPrefab = prefabLibrary.waterPrefab;
        networkServer.windPrefab = prefabLibrary.windPrefab;

        // STEP 6: เตรียม Payload (รวม characterName ที่เลือกไว้)
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId,
            characterName = PlayerPrefs.GetString("SelectedCharacter", "Water") // ✅ เอาจาก PlayerPrefs
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        // STEP 7: เริ่ม Host!
        NetworkManager.Singleton.StartHost();

        // (ถ้าจะเปลี่ยน Scene ให้ไปหลัง spawn player แล้ว)
        // NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public async void Dispose()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));

        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }

            lobbyId = string.Empty;
        }

        networkServer?.Dispose();
    }
}
