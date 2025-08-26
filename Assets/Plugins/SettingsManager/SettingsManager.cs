using System.IO;
using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class SettingsManager : MonoBehaviour
{

    [SerializeField] private string settingsFileName = "Settings.json";
    [SerializeField] public Settings settings;
    [ShowInInspector, ReadOnly] private string settingsFilePath;

    public static event Action<Settings> OnSettingsLoaded;
    public static SettingsManager instance;

    private void Awake()
    {
        if (instance == null)
        { instance = this; }
        else
        { Debug.LogWarning("multiple settings managers detected in scene"); }
        settingsFilePath = Path.Combine(Application.streamingAssetsPath, settingsFileName);
    }

    private void Start()
    {
        if (settings != null)
        {
            ReadSettingsFile();
        }
        else
        {
            Debug.LogError("ScriptableObject settings reference not assigned!");
        }
    }

    [Button]
    private void ReadSettingsFile()
    {
        if (!File.Exists(settingsFilePath))
        {
            Debug.Log("<color=red>Settings file read error.</color>");
            SaveSettingsFile();
        }

        using (StreamReader r = new StreamReader(settingsFilePath))
        {
            string text = r.ReadToEnd();
            r.Close();
            JsonUtility.FromJsonOverwrite(text, settings);

            if (settings != null)
                OnSettingsLoaded?.Invoke(settings);
        }
    }

    [Button]
    public void SaveSettingsFile()
    {
        try
        {
            using (var w = new StreamWriter(settingsFilePath))
            {
                string json = JsonUtility.ToJson(settings);
                w.Write(json);
                Debug.Log("<color=green>Settings json saved.</color>");
            }
        }
        catch (Exception ex)
        {
            Debug.Log("<color=red>Settings file save error: </color>" + ex);
        }
    }
}