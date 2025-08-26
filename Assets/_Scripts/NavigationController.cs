using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavigationController : MonoBehaviour
{
    public static NavigationController instance;
    public List<GameObject> pages;
    public int currentPage = 0;
    public List<Button> buttons;
    public Texture2D selected;
    public Texture2D notSelected;

    [Header("Click Limit Settings")]
    [Tooltip("Minimum time (in seconds) between navigation clicks")]
    public float clickCooldown = 1f;

    private float lastClickTime = -Mathf.Infinity;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        OpenInstantPage(0);
        HighLightButton(-1); // reset button highlight
    }

    private bool CanClick()
    {
        if (Time.unscaledTime - lastClickTime < clickCooldown)
        {
            Debug.Log("Navigation click blocked - too fast!");
            return false;
        }
        lastClickTime = Time.unscaledTime;
        return true;
    }

    public void OpenPage(int index)
    {
        if (!CanClick()) return;

        for (int i = 0; i < pages.Count; i++)
        {
            if (i == index)
            {
                pages[i].SetActive(true);
            }
            else
            {
                pages[i].GetComponent<Fader>().FadeOutTurnOff();
            }
        }
        currentPage = index;
    }

    public void OpenInstantPage(int index)
    {
        if (!CanClick()) return;

        for (int i = 0; i < pages.Count; i++)
        {
            if (i == index)
            {
                pages[i].SetActive(true);
            }
            else
            {
                pages[i].SetActive(false);
            }
        }
        currentPage = index;
    }

    public void BackPage()
    {
        if (!CanClick()) return;
        OpenPage(--currentPage);
    }

    public void BackToOptions()
    {
        if (!CanClick()) return;
        OpenPage(1);
    }

    public void RestartFlow()
    {
        if (!CanClick()) return;
        OpenPage(0);
    }

    public void HighLightButton(int index)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (i == index)
            {
                buttons[i].GetComponent<RawImage>().texture = selected;
            }
            else
            {
                buttons[i].GetComponent<RawImage>().texture = notSelected;
            }
        }
    }
}
