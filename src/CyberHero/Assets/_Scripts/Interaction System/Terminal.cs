using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terminal : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt;
    [SerializeField] GameObject terminalPanel;

    public string InteractionPrompt => prompt;

    private void Awake()
    {
        terminalPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public bool Interact(Interactor interactor)
    {
        terminalPanel.SetActive(true);
        Time.timeScale = 0f;
        return true;
    }
}
