using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelect : NetworkBehaviour
{
    private Dictionary<ulong, bool> readyStatus = new();

    public void Ready()
    {
        SubmitReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        readyStatus[clientId] = true;

        Debug.Log($"✅ Client {clientId} is ready.");

        if (AllPlayersReady())
        {
            Debug.Log("🎯 All players ready! Loading Lv.1 scene...");
            NetworkManager.Singleton.SceneManager.LoadScene("Lv.1", LoadSceneMode.Single);
        }
    }

    private bool AllPlayersReady()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!readyStatus.ContainsKey(client) || !readyStatus[client])
                return false;
        }
        return true;
    }
}
