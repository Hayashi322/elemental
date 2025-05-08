using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Analytics;

public class GameRoundManager : NetworkBehaviour
{
    public static GameRoundManager Instance;

    public NetworkVariable<int> NetworkRound = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone);
    private Dictionary<ulong, int> playerScores = new();
    private const int maxWins = 2;
    private int currentRound = 1;

    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string loseSceneName = "LoseScene";

    private ulong lastWinnerClientId;
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
        if (winnerClientId == ulong.MaxValue)
        {
            Debug.LogWarning("⚠️ Opponent left — auto win for remaining player.");
            foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
            {
                if (clientId != deadClientId)
                {
                    winnerClientId = clientId;
                    break;
                }
            }

            if (winnerClientId == ulong.MaxValue)
            {
                Debug.LogError("❌ No remaining players found.");
                return;
            }
        }

        if (!playerScores.ContainsKey(winnerClientId))
            playerScores[winnerClientId] = 0;

        playerScores[winnerClientId]++;
        Debug.Log($"🎯 Player {winnerClientId} wins a round! Total wins: {playerScores[winnerClientId]}");

        if (playerScores[winnerClientId] >= maxWins)
        {
            Debug.Log($"🏆 Player {winnerClientId} wins the match!");
            lastWinnerClientId = winnerClientId;

            var winnerData = HostSingleton.Instance.Server.GetUserDataByClientId(winnerClientId);
            if (winnerData != null)
            {
                AnalyticsService.Instance.RecordEvent(new CustomEvent("match_winner")
                {
                    { "character_name", winnerData.characterName }
                });

                Debug.Log("✅ Analytics event sent for: " + winnerData.characterName);
            }
            else
            {
                Debug.LogWarning("⚠️ winnerData is null. Cannot send analytics.");
            }

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

    public bool IsWinner(ulong clientId)
    {
        return clientId == lastWinnerClientId;
    }

    private void ReloadBattleScene()
    {
        Debug.Log("🔁 Reloading Lv.1...");
        NetworkManager.SceneManager.LoadScene("Lv.1", LoadSceneMode.Single);
    }

    private ulong GetOpponent(ulong deadClientId)
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            if (clientId != deadClientId)
                return clientId;
        }
        Debug.LogError("❌ No opponent found!");
        return ulong.MaxValue;
    }

    private void LoadResultScenes(ulong winnerClientId)
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            string sceneName = (clientId == winnerClientId) ? winSceneName : loseSceneName;
            HostSingleton.Instance.StartCoroutine(DelayedSceneLoad(sceneName, clientId));
        }
    }

    private IEnumerator DelayedSceneLoad(string sceneName, ulong clientId)
    {
        yield return new WaitForSeconds(1.0f);

        SendLoadSceneClientRpc(sceneName, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        });
    }

    [ClientRpc]
    private void SendLoadSceneClientRpc(string sceneName, ClientRpcParams clientRpcParams = default)
    {
        SceneManager.LoadScene(sceneName);
    }
}