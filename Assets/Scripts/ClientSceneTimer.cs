using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Analytics;
using System.Collections.Generic;
using Unity.Netcode;

public class ClientSceneTimer : MonoBehaviour
{
    private float enterTime;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lv.1" && NetworkManager.Singleton.IsClient)
        {
            enterTime = Time.realtimeSinceStartup;
            Debug.Log("⏱️ Client started Lv.1");
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "Lv.1" && NetworkManager.Singleton.IsClient)
        {
            float duration = Time.realtimeSinceStartup - enterTime;

            AnalyticsService.Instance.RecordEvent(new CustomEvent("scene_duration")
            {
                { "duration_seconds", duration }
            });

            Debug.Log($"📊 Sent Lv.1 duration: {duration} sec for Client {NetworkManager.Singleton.LocalClientId}");
        }
    }
}
