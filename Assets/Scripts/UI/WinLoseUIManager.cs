using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class WinLoseUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Buttons")]
    public Button lobbyButton;
    public Button exitButton;

    private IEnumerator Start()
    {
        // 🔁 รอจน GameRoundManager พร้อม
        while (GameRoundManager.Instance == null)
            yield return null;

        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        bool isWinner = GameRoundManager.Instance.IsWinner(localClientId);

        winPanel.SetActive(isWinner);
        losePanel.SetActive(!isWinner);

        lobbyButton.onClick.AddListener(OnLobbyPressed);
        exitButton.onClick.AddListener(OnExitPressed);
    }

    private void OnLobbyPressed()
    {
        // 🔌 ปิด Netcode ก่อนกลับเมนู (ถ้า Host/Client ยังรันอยู่)
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene("MainMenu");
    }

    private void OnExitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
