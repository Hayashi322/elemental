using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameRoundManager : NetworkBehaviour
{
    public static GameRoundManager Instance;

    public NetworkVariable<int> NetworkRound = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone);

    private Dictionary<ulong, int> playerScores = new();
    private const int maxWins = 2;
    private int currentRound = 1;

    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string loseSceneName = "LoseScene";

    public Action<int> OnRoundChanged;

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

    public void OnPlayerDied(ulong deadClientId)
    {
        if (!IsServer) return;

        ulong winnerClientId = GetOpponent(deadClientId);
        if (winnerClientId == deadClientId || winnerClientId == 0)
        {
            Debug.LogError("❌ No opponent found! Cannot assign win.");
            return;
        }

        if (!playerScores.ContainsKey(winnerClientId))
        {
            playerScores[winnerClientId] = 0;
        }
        playerScores[winnerClientId]++;

        Debug.Log($"🎯 Player {winnerClientId} wins a round! Total wins: {playerScores[winnerClientId]}");

        if (playerScores[winnerClientId] >= maxWins)
        {
            Debug.Log($"🏆 Player {winnerClientId} wins the match!");
            LoadResultScenes(winnerClientId);
        }
        else
        {
            currentRound++;
            NetworkRound.Value = currentRound;
            OnRoundChanged?.Invoke(currentRound);
            ReloadBattleScene();
        }
    }

    private void ReloadBattleScene()
    {
        Debug.Log("🔁 Reloading Lv.1...");
        NetworkManager.SceneManager.LoadScene("Lv.1", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private ulong GetOpponent(ulong deadClientId)
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            if (clientId != deadClientId)
                return clientId;
        }
        return 0;
    }

    private void LoadResultScenes(ulong winnerClientId)
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            string sceneName = (clientId == winnerClientId) ? winSceneName : loseSceneName;
            LoadSceneForClientRpc(sceneName, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });
        }
    }

    [ClientRpc]
    private void LoadSceneForClientRpc(string sceneName, ClientRpcParams clientRpcParams = default)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
