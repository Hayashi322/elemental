using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;
    // ไม่ต้องมี hostPrefab ถ้าใช้ HostSingleton จาก Scene
    // [SerializeField] private HostSingleton hostPrefab;

    async void Start()
    {
        DontDestroyOnLoad(gameObject);
        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            // สำหรับโหมดเซิร์ฟเวอร์ล้วน
        }
        else
        {
            // ใช้ HostSingleton ที่มีอยู่ใน Scene (เช่น HostManager)
            HostSingleton hostSingleton = HostSingleton.Instance;
            hostSingleton.CreateHost();

            // Instantiate client ได้ตามปกติ
            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            bool authenticated = await clientSingleton.CreateClient();

            if (authenticated)
            {
                clientSingleton.GameManager.GoToMenu();
            }
        }
    }
}
