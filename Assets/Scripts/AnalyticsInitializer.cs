using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;
using System.Threading.Tasks;

public class AnalyticsInitializer : MonoBehaviour
{
    public static AnalyticsInitializer Instance { get; private set; }

    private async void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeAnalytics();
    }

    private async Task InitializeAnalytics()
    {
        try
        {
            await UnityServices.InitializeAsync();

            AnalyticsService.Instance.StartDataCollection();
            Debug.Log("✅ Analytics Initialized & Data Collection Started.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Analytics initialization failed: {e.Message}");
        }
    }
}
