using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private GameObject uiPanel;

    public bool IsDisplayed = false;

    void Start()
    {
        uiPanel.SetActive(false);
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }


    public void SetUp(string _promptText)
    {
        promptText.text = _promptText;
        uiPanel.SetActive(true);
        IsDisplayed = true;
    }

    public void Close()
    {
        IsDisplayed = false;
        uiPanel.SetActive(false);  
    }

}
