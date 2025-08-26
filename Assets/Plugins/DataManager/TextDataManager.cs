using RTLTMPro;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TextDataManager : MonoBehaviour
{
    [Header("Remote CSV Download Settings")]
    [SerializeField] private string dataUrl = "https://docs.google.com/spreadsheets/d/1234/export?format=csv";
    [SerializeField] private string dataFileName = "Data.csv";

    [Header("Data Download Key (CTRL + F5)")]
    [SerializeField] private KeyCode downloadDataKey = KeyCode.F5;

    [Header("Data Save Key (CTRL + F6)")]
    [Tooltip("Press CTRL + F6 to find all DataItems in the scene and save their data into CSV.")]
    [SerializeField] private KeyCode saveDataKey = KeyCode.F6;

    [Header("Arabic")]
    [ShowInInspector, ReadOnly] public bool isArabic = false;

    [Header("Data Storage")]
    [SerializeField] private List<TextDataModel> dataList;
    public static TextDataManager instance;
    public static Action OnDataLoaded;

    private string settingsFileFolder = Application.streamingAssetsPath;
    private string dataFilePath;

    private void OnEnable()
    {
        TextDataFileDownloader.OnFileDownloaded += LoadLocalData;
    }

    private void OnDisable()
    {
        TextDataFileDownloader.OnFileDownloaded -= LoadLocalData;
    }

    private void Awake()
    {
        instance = this;
        ExtractDataURL();
        LoadLocalData();
    }

    private void ExtractDataURL()
    {
        // Searches the current dataUrl variable for the sheet ID and reconstructs the export link.
        const string marker = "/d/";
        int startIndex = dataUrl.IndexOf(marker);

        // 1. Check if the URL is a valid Google Sheet link containing "/d/"
        if (startIndex == -1)
        {
            Debug.LogError("Invalid Google Sheet URL format: The URL does not contain '/d/'.");
            return;
        }

        // 2. Adjust startIndex to be right after "/d/"
        startIndex += marker.Length;

        // 3. Find the end of the sheet ID, marked by the next '/'
        int endIndex = dataUrl.IndexOf('/', startIndex);
        if (endIndex == -1)
        {
            // Handle cases where the URL might end after the ID without a trailing slash
            // For example: https://docs.google.com/spreadsheets/d/1234
            // In this scenario, we might try to extract up to the end of the string or until a '?'
            int queryIndex = dataUrl.IndexOf('?', startIndex);
            if (queryIndex != -1)
            {
                endIndex = queryIndex;
            }
            else
            {
                Debug.LogError("Invalid Google Sheet URL format: Could not find the closing '/' or '?' after the sheet ID.");
                return;
            }
        }

        // 4. Extract the sheet ID
        string sheetId = dataUrl.Substring(startIndex, endIndex - startIndex);

        // 5. Reconstruct the URL into the correct CSV export format.
        // The 'gid=0' parameter exports the very first sheet in the document.
        dataUrl = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv";

        Debug.Log($"Reformatted data URL to: {dataUrl}");
    }

    private void Update()
    {
        // Press CTRL + F5 to download fresh data from the Google Sheet
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            && Input.GetKeyDown(downloadDataKey))
        {
            DownloadData();
        }

        // Press CTRL + F6 to save current scene data to a CSV
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            && Input.GetKeyDown(saveDataKey))
        {
            SaveLocalData();
        }
    }

    public void DownloadData()
    {
        dataFilePath = Path.Combine(settingsFileFolder, dataFileName);
        Debug.Log("Downloading data: " + dataFilePath);
        TextDataFileDownloader.instance.DownloadFile(dataUrl, dataFilePath, dataFileName);
    }

    public void LoadLocalData()
    {
        StartCoroutine(LoadLocalDataSeq());
    }


    private IEnumerator LoadLocalDataSeq()
    {
        dataFilePath = Path.Combine(settingsFileFolder, dataFileName);

        if (!File.Exists(dataFilePath))
        {
            Debug.Log($"Text Data Manager: File not found at {dataFilePath}");
        }
        else
        {
            using (StreamReader r = new StreamReader(dataFilePath))
            {
                Debug.Log("Text Data Manager: File read successful");
                string text = r.ReadToEnd();
                r.Close();

                dataList = CSVSerializer.Deserialize<TextDataModel>(text).ToList();
            }

            yield return null;

            if (dataList != null)
            {
                Debug.Log("Text Data Manager: Data parse successful");
                OnDataLoaded?.Invoke();
            }
        }
    }

    /// <summary>
    /// Collect all DataItem objects in the scene, gather their ID/text,
    /// and save them into a CSV file in the StreamingAssets folder.
    /// </summary>
    private void SaveLocalData()
    {
        Debug.Log("Text Data Manager: Saving scene data locally");

        TextDataItem[] dataItems = FindObjectsByType<TextDataItem>(FindObjectsSortMode.InstanceID);
        List<TextDataModel> currentSceneData = new List<TextDataModel>();

        foreach (TextDataItem item in dataItems)
        {
            // Use the public 'id' field on the DataItem
            string itemId = item.id;
            string itemText;
            // Use the RTLTextMeshProUGUI's 'text' field on the DataItem
            if (item.isArabic)
            {
                itemText = ((RTLTextMeshPro)item.text).OriginalText;
            }
            // Use the TextMeshProUGUI's 'text' field on the DataItem
            else
            {
                itemText =  item.text.text;
            }

            // Use the TextMeshProUGUI's 'text' field on the DataItem
            // Create a DataModelExcel entry
            currentSceneData.Add(new TextDataModel
            {
                id = itemId,
                text = itemText
            });
        }

        // Convert to CSV (or however you are saving) without using the plugin
        string csvString = ConvertToCsv(currentSceneData);

        dataFilePath = Path.Combine(settingsFileFolder, dataFileName);

        try
        {
            File.WriteAllText(dataFilePath, csvString);
            Debug.Log($"Text Data Manager: Data saved successfully to {dataFilePath}.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Text Data Manager: Failed to save data to {dataFilePath}. Exception: {e.Message}");
        }
    }

    /// <summary>
    /// Returns a DataModelExcel entry matching the given ID, or null if not found.
    /// </summary>
    public TextDataModel GetDataByID(string id)
    {
        return dataList.FirstOrDefault(x => x.id.Equals(id));
    }

    /// <summary>
    /// Converts a list of DataModelExcel to a CSV-formatted string manually.
    /// (First line is the header, subsequent lines are the records.)
    /// </summary>
    private string ConvertToCsv(List<TextDataModel> data)
    {
        // You can expand the escaping logic as needed to handle
        // special characters (commas, quotes, newlines, etc.)
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // CSV Header
        sb.AppendLine("id,text");

        // CSV Rows
        foreach (TextDataModel entry in data)
        {
            string safeId = EscapeForCsv(entry.id);
            string safeText = EscapeForCsv(entry.text);

            sb.AppendLine($"{safeId},{safeText}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Escapes CSV cells that might contain quotes, commas, or newlines.
    /// </summary>
    private string EscapeForCsv(string str)
    {
        if (str == null) str = string.Empty;
        bool needQuotes = str.Contains(",") || str.Contains("\"") || str.Contains("\n") || str.Contains("\r");

        // Replace existing quotes with doubled quotes
        str = str.Replace("\"", "\"\"");

        // Wrap in quotes if needed
        if (needQuotes)
        {
            str = $"\"{str}\"";
        }

        return str;
    }
}
