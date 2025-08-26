using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InactivityDetector : MonoBehaviour
{
    [SerializeField] private float inactivityTimer = 0f;
    public static InactivityDetector Instance;
    [SerializeField] private float inactivityThreshold;
    [SerializeField] private bool isUserActive = true;
    [SerializeField] private bool isActive = true;

    [System.Serializable]
    public class SettingsData
    {
        public float inactivityThreshold;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        LoadSettings();
    }

    void LoadSettings()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "settings.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SettingsData data = JsonUtility.FromJson<SettingsData>(json);
            inactivityThreshold = data.inactivityThreshold;
        }
        else
        {
            Debug.LogWarning("settings.json not found, using default threshold");
            inactivityThreshold = 180f; // fallback (3 minutes)
        }
    }

    void Update()
    {
        if (isActive)
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 || Input.anyKey)
            {
                inactivityTimer = 0;
                isUserActive = true;
            }
            else
            {
                inactivityTimer += Time.deltaTime;
                if (inactivityTimer >= inactivityThreshold && isUserActive)
                {
                    try
                    {
                        PagesController.Instance.RestartFlow();
                    }
                    catch { }
                    isUserActive = false;
                }
            }
        }
    }

    public void ResetTimer()
    {
        inactivityTimer = 0;
        isUserActive = true;
    }
    public void Activate() { isActive = true; }
    public void DeActivate() { isActive = false; }
}
