using System;
using System.Threading.Tasks;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton instance;

    public HostGameManager GameManager { get; private set; }

    public CharacterPrefabLibrary prefabLibrary;



    public static HostSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<HostSingleton>();

            if (instance == null)
            {
                Debug.LogError("❌ No HostSingleton in the scene!");
                return null;
            }

            Debug.Log("🔍 HostSingleton found on GameObject: " + instance.gameObject.name);
            return instance;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateHost()
    {
        if (GameManager != null)
        {
            Debug.Log("⚠️ HostGameManager already created, skipping CreateHost()");
            return;
        }

        GameManager = new HostGameManager(prefabLibrary);
    }


    private void OnDestroy()
    {
        GameManager?.Dispose();
    }

}