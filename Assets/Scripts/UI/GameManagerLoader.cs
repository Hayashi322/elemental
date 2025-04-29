using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerLoader : MonoBehaviour
{
    [SerializeField] private GameRoundManager gameRoundManagerPrefab;

    private static bool hasSpawned = false;

    private void Awake()
    {
        if (hasSpawned)
        {
            Destroy(gameObject); // มีแล้ว ไม่ต้องสร้างใหม่
            return;
        }

        DontDestroyOnLoad(gameObject);

        if (GameRoundManager.Instance == null && gameRoundManagerPrefab != null)
        {
            var go = Instantiate(gameRoundManagerPrefab);
            DontDestroyOnLoad(go);
            Debug.Log("✅ GameRoundManager created and marked as DontDestroyOnLoad.");
        }

        hasSpawned = true;
    }
}
