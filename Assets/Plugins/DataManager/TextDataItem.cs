using RTLTMPro;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class TextDataItem : MonoBehaviour
{
    [ShowInInspector,ReadOnly] public string id = "";
    [ShowInInspector, ReadOnly] public TextMeshProUGUI text;
    public bool isArabic;

    [Space(10)]
    [ShowInInspector, ReadOnly]
    public TextDataModel dataModel;

    void Awake()
    {
        id = gameObject.name;
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (TextDataManager.instance != null)
        {
            InitializeData();
        }
        TextDataManager.OnDataLoaded += InitializeData;
    }

    private void OnDisable()
    {
        TextDataManager.OnDataLoaded -= InitializeData;
    }

    void InitializeData()
    {
        dataModel = TextDataManager.instance.GetDataByID(id);

        if (text != null)
        {
            if (isArabic)
            {
                ((RTLTextMeshPro)text).text = dataModel.text;
            }

            else
            {
                text.text = dataModel.text;
            }
        }
        
        else
        {
            Debug.LogWarning("Missing text mesh component on " + id);
        }
    }

}