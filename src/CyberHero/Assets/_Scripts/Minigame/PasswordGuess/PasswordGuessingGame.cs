using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordGuessingGame : MonoBehaviour
{
    public Button[] passwordButtons;
    public TextMeshProUGUI timerText;
    public float timeLimit = 10f;

    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    public Button retryButton;
    public Button nextButton;

    [Header("Next Minigame")]
    public GameObject nextMinigame;

    private string[] passwords = new string[]
   {
        "password123",
        "Pa$$w0rd!",
        "123456",
        "admin"
   };

    private int correctPasswordIndex = 1;

    private void OnEnable()
    {
        SetupGame();
    }

    void SetupGame()
    {

        if (resultText == null)
        {
            Debug.LogError("resultText is not assigned!");
            return;
        }
        resultText.text = "";

        for (int i = passwords.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = passwords[i];
            passwords[i] = passwords[j];
            passwords[j] = temp;
        }


        for (int i = 0; i < passwordButtons.Length; i++)
        {
            if (passwordButtons[i] == null)
            {
                Debug.LogError($"passwordButton at index {i} is not assigned!");
                continue;
            }


            int index = i; 
            TextMeshProUGUI buttonText = passwordButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText == null)
            {
                Debug.LogError($"Text component is missing in passwordButton at index {i}!");
                continue;
            }

            buttonText.text = passwords[i];

            passwordButtons[i].onClick.AddListener(() => CheckPassword(index));
        }


        StartCoroutine(StartTimer());
    }

    IEnumerator StartTimer()
    {
        float currentTime = timeLimit;
        while (currentTime > 0)
        {
            timerText.text = "Time: " + currentTime.ToString("0");
            yield return new WaitForSecondsRealtime(1f);
            currentTime--;
        }

        timerText.text = "Time: 0";
        resultText.text = "Waktu habis! Coba lagi.";
    }

    void CheckPassword(int index)
    {
        if (resultPanel == null || resultText == null || retryButton == null || nextButton == null)
        {
            Debug.LogError("One or more UI elements are not assigned!");
            return;
        }

        resultPanel.SetActive(true);


        if (index == correctPasswordIndex)
        {
            resultText.text = "Selamat! Anda memilih kata sandi yang kuat.";
            nextButton.gameObject.SetActive(true);
            retryButton.gameObject.SetActive(false);
            nextButton.onClick.AddListener(() => NextMinigame());
        }
        else
        {
            resultText.text = "Kata sandi lemah. Coba lagi.";
            retryButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(false);
            retryButton.onClick.AddListener(() => RestartMinigame());
        }


        StopAllCoroutines();
        foreach (Button btn in passwordButtons)
        {
            btn.onClick.RemoveAllListeners();
        }
    }

    void NextMinigame()
    {
        nextMinigame.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void RestartMinigame()
    {
        resultPanel.SetActive(false);
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
    }
}
