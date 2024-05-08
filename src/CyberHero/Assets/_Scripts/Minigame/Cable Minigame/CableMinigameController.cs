using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CableMinigameController : MonoBehaviour
{
    public GameObject CablesHolder;
    public GameObject[] Cables;

    public GameObject minigameOverPanel;
    public GameObject terminalPanel;
    public TextMeshProUGUI textUI;
    public string text;

    [SerializeField] int totalCables = 0;

    [SerializeField] int correctedCables = 0;

    void Start()
    {
        totalCables = CablesHolder.transform.childCount;

        Cables = new GameObject[totalCables];

        for (int i = 0; i < Cables.Length; i++)
        {
            Cables[i] = CablesHolder.transform.GetChild(i).gameObject;
        }
    }

    public void correctMove()
    {
        correctedCables += 1;

        if (correctedCables == totalCables)
        {
            StartCoroutine(ContinueGame());
        }
    }

    IEnumerator ContinueGame()
    {
        textUI.text = text;
        minigameOverPanel.SetActive(true);
        Time.timeScale = 1f;
        yield return new WaitForSeconds(1);
        minigameOverPanel.SetActive(false);
        terminalPanel.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void wrongMove()
    {
        correctedCables -= 1;
    }
}
