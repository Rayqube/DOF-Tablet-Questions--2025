using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class TextDataFileDownloader : MonoBehaviour
{
    public static TextDataFileDownloader instance;

    public static Action OnFileDownloaded;

    private void Awake()
    {
        instance = this;
    }

    public void DownloadFile(string _path, string _downloadedFilePath, string _fileName)
    {
        StartCoroutine(GetRequest(_path, _downloadedFilePath, _fileName));
    }

    IEnumerator GetRequest(string uri, string downloadedFilePath, string filename)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                throw new Exception($"Https Error {webRequest.error} url:{webRequest.url}");
            }

            if (File.Exists(downloadedFilePath))
            {
                File.Delete(downloadedFilePath);
            }

            File.WriteAllText(downloadedFilePath, webRequest.downloadHandler.text);

            OnFileDownloaded?.Invoke(); 

            webRequest.Dispose();
        }
    }
}
