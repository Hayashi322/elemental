using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelect : NetworkBehaviour
{
    public void SelectCharacter(string characterName)
    {
        Debug.Log("🟡 SelectCharacter called with: " + characterName);

        // 🔥 เช็คแค่ว่าเราเป็น Host (เจ้าของ Server) หรือไม่
        if (IsHost)
        {
            SubmitCharacterSelectionServerRpc(characterName);
        }
        else
        {
            SubmitCharacterSelectionServerRpc(characterName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitCharacterSelectionServerRpc(string characterName, ServerRpcParams rpcParams = default)
    {
        Debug.Log("🟢 ServerRpc called with character: " + characterName);

        var clientId = rpcParams.Receive.SenderClientId;
        var userData = HostSingleton.Instance.GameManager.Server.GetUserDataByClientId(clientId);

        if (userData != null)
        {
            userData.characterName = characterName;
            Debug.Log("✅ Character updated to: " + characterName);
            NetworkManager.Singleton.SceneManager.LoadScene("Lv.1", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("⚠️ userData not found!");
        }
    }
}
