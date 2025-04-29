using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RoundUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roundText;

    private void Start()
    {
        if (GameRoundManager.Instance != null && GameRoundManager.Instance.IsSpawned)
        {
            UpdateRound(GameRoundManager.Instance.NetworkRound.Value);
            GameRoundManager.Instance.NetworkRound.OnValueChanged += OnRoundChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameRoundManager.Instance != null && GameRoundManager.Instance.IsSpawned)
        {
            GameRoundManager.Instance.NetworkRound.OnValueChanged -= OnRoundChanged;
        }
    }

    private void OnRoundChanged(int oldValue, int newValue)
    {
        UpdateRound(newValue);
    }

    private void UpdateRound(int round)
    {
        roundText.text = $"Round {round}";
    }
}
