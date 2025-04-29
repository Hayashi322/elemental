using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Analytics;
using System.Collections.Generic;

public class CharacterSelect : NetworkBehaviour
{
    async void Awake()
    {
        await UnityServices.InitializeAsync();
    }

    public void SelectCharacter(string characterName)
    {
        Debug.Log("🟡 SelectCharacter called with: " + characterName);
        if (IsClient)
        {
            SubmitCharacterSelectionServerRpc(characterName);
        }
    }

    public void Ready()
    {
        Debug.Log("🟡 Ready button pressed");
        if (IsClient)
        {
            SubmitReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitCharacterSelectionServerRpc(string characterName, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        var userData = HostSingleton.Instance.GameManager.Server.GetUserDataByClientId(clientId);

        if (userData != null)
        {
            userData.characterName = characterName;
            Debug.Log($"✅ Character updated to: {characterName}");

            // ✅ ใช้ CustomEvent แบบนี้
            var customEvent = new CustomEvent("character_selected");
            customEvent["player_id"] = clientId.ToString(); // หรือจะใช้เป็น int ก็ได้ ถ้าแน่ใจว่าไม่เกินขนาด
            customEvent["character_name"] = characterName;

            AnalyticsService.Instance.RecordEvent(customEvent);

            Debug.Log($"📊 Analytics Event Sent: {characterName} by Player {clientId}");
        }
        else
        {
            Debug.LogWarning("⚠️ userData not found!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        var userData = HostSingleton.Instance.GameManager.Server.GetUserDataByClientId(clientId);

        if (userData != null)
        {
            userData.isReady = true;
            Debug.Log($"✅ Client {clientId} is Ready.");

            if (HostSingleton.Instance.GameManager.Server.AreAllPlayersReady())
            {
                Debug.Log("🚀 All players are Ready! Loading Lv.1...");
                NetworkManager.Singleton.SceneManager.LoadScene("Lv.1", LoadSceneMode.Single);
            }
        }
        else
        {
            Debug.LogWarning("⚠️ userData not found for Ready!");
        }
    }
}
