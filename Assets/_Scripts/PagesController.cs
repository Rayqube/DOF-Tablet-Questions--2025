using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PagesController : MonoBehaviour
{
    public static PagesController Instance;
    public List<GameObject> pages;
    public int currentPage = 0;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
        //DontDestroyOnLoad(this);

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    private void OnEnable()
    {
        if (GameInstance.instance != null)
        {
            if (GameInstance.instance.isNewGame)
            {
                OpenPage(0);
            }
            else
            {
                OpenPage(2);
                DOVirtual.DelayedCall(5, delegate { OpenPage(0); });
            }
        }
        else { OpenPage(0); }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OpenPage(int index)
    {
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
    public void RestartFlow()
    {
        OpenPage(0);

        InactivityDetector.Instance.ResetTimer();
        InactivityDetector.Instance.Activate();
    }
}
