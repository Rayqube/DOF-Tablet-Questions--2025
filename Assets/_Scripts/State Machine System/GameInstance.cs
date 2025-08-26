using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInstance : MonoBehaviour
{
    #region Singleton
    public static GameInstance instance;

    public bool isNewGame = true;
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SettingsManager.OnSettingsLoaded += Setup;
        }
    }

    #endregion

    void Setup(Settings settings)
    {
        Application.targetFrameRate = settings.frameRate;
        QualitySettings.vSyncCount = settings.vSync;
    }

    void OnApplicationQuit()
    {
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
