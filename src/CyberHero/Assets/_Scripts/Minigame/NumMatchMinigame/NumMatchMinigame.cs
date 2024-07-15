using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NumMatchMinigame : MonoBehaviour
{
    public TextMeshProUGUI targetNumberText;
    public TextMeshProUGUI rollingNumberText;
    public Button stopButton;
    [SerializeField]
    private float rollSpeed = 0.1f;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button restartButton;
    public Button finishButton;

    private int targetNumber;
    private int rollingNumber;
    private bool isRolling = true;
    

    void OnEnable()
    {
        targetNumber = Random.Range(0, 10);
        targetNumberText.text = targetNumber.ToString();


        stopButton.onClick.AddListener(StopRolling);
        StartCoroutine(RollingNumbers());
    }

    IEnumerator RollingNumbers()
    {
        while (isRolling)
        {
          
            rollingNumber = Random.Range(0, 10);
            rollingNumberText.text = rollingNumber.ToString();
            yield return new WaitForSecondsRealtime(rollSpeed);
        }
    }

    void StopRolling()
    {
       
        isRolling = false;
        resultPanel.gameObject.SetActive(true);
        
        if (rollingNumber == targetNumber)
        {
            resultText.text = "Berhasil! Angka sesuai.";
            finishButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(false);
            finishButton.onClick.AddListener(Finish);
           
        }
        else
        {
            resultText.text = "Coba Lagi! Angka tidak sesuai.";
            finishButton.gameObject.SetActive(false);
            restartButton.gameObject.SetActive(true);
            restartButton.onClick.AddListener(RestartMinigame);
            
        }
    }

    void RestartMinigame()
    {
        resultPanel.gameObject.SetActive(false);
        isRolling = true;
        this.gameObject.SetActive (false);
        this.gameObject.SetActive(true);
    }

    void Finish()
    {
        this.gameObject.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
