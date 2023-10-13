using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Minigame : MonoBehaviour
{
    [Header("True Answer")]
    [SerializeField] Question[] questions;
    private static List<Question> unAnsweredQuestionsList;

    [Header("False Answer")]
    [SerializeField] Question[] falseQuestion;
    private static List<Question> unAnsweredFalseQuestionsList;

    [Header("UI")]
    [SerializeField] private Canvas questionCanvas;
    [SerializeField] private TMP_Text factText;
    [SerializeField] private TMP_Text factFalseText;

    [Header(header: "UI Finished")]
    [SerializeField] Canvas finishedCanvas;
    [SerializeField] TMP_Text finishedText;

    private Question currentQuestion;
    private Question currentFalseQuestion;
    private string buttonName;
    [SerializeField] private float timeBetweenQuestion = 1f;

    // [Header("After Answer Timer")]
    // [SerializeField] private float timer = 5;
    // private bool isTimerRunning = false;

    [Header("Firewall")]
    [SerializeField] List<FirewallHealth> firewall = new List<FirewallHealth>();
    [SerializeField] List<GameObject> firewallButton = new List<GameObject>();
    public StateFirewall1 stateFirewall1;
    public StateFirewall2 stateFirewall2;
    public StateFirewall3 stateFirewall3;

    private int firewall1AnsweredQuestionCount;
    private int firewall2AnsweredQuestionCount;
    private int firewall3AnsweredQuestionCount;
    private int answeredQuestion = 0;

    public enum StateFirewall1
    {
        Phase0,
        Phase1,
        Phase2,
        Phase3
    }
    public enum StateFirewall2
    {
        Phase0,
        Phase1,
        Phase2,
        Phase3
    }
    public enum StateFirewall3
    {
        Phase0,
        Phase1,
        Phase2,
        Phase3,
        PhaseWin
    }


    private void Update()
    {
        switch (stateFirewall1)
        {
            case StateFirewall1.Phase0:
                firewall1AnsweredQuestionCount = 0;

                stateFirewall1 = StateFirewall1.Phase1;
                break;

            case StateFirewall1.Phase1:
        
                if (firewall1AnsweredQuestionCount == 1)
                {
                    stateFirewall1 = StateFirewall1.Phase2;
                }

                break;

            case StateFirewall1.Phase2:
                if (firewall1AnsweredQuestionCount == 2)
                {
                    stateFirewall1 = StateFirewall1.Phase3;
                }
                break;

            case StateFirewall1.Phase3:
                if (firewall1AnsweredQuestionCount == 3)
                {
                    firewall[0].GetComponent<FirewallHealth>().enabled = false;
                    firewallButton[0].gameObject.SetActive(false);
                }
                break;
        }

        switch (stateFirewall2)
        {
            case StateFirewall2.Phase0:
                firewall2AnsweredQuestionCount = 0;

                stateFirewall2 = StateFirewall2.Phase1;
                break;

            case StateFirewall2.Phase1:
                if (firewall2AnsweredQuestionCount == 1)
                {
                    stateFirewall2 = StateFirewall2.Phase2;
                }

                break;

            case StateFirewall2.Phase2:
                if (firewall2AnsweredQuestionCount == 2)
                {
                    stateFirewall2 = StateFirewall2.Phase3;
                }
                break;

            case StateFirewall2.Phase3:
                if (firewall2AnsweredQuestionCount == 3)
                {
                    firewall[1].GetComponent<FirewallHealth>().enabled = false;
                    firewallButton[1].gameObject.SetActive(false);
                }
                break;
        }

        switch (stateFirewall3)
        {
            case StateFirewall3.Phase0:
                firewall3AnsweredQuestionCount = 0;

                stateFirewall3 = StateFirewall3.Phase1;
                break;

            case StateFirewall3.Phase1:
                if (firewall3AnsweredQuestionCount == 1)
                {
                    stateFirewall3 = StateFirewall3.Phase2;
                }

                break;

            case StateFirewall3.Phase2:
                if (firewall3AnsweredQuestionCount == 2)
                {

                    stateFirewall3 = StateFirewall3.Phase3;
                }
                break;

            case StateFirewall3.Phase3:
                if (firewall3AnsweredQuestionCount == 3)
                {
                    firewall[2].GetComponent<FirewallHealth>().enabled = false;
                    firewallButton[2].gameObject.SetActive(false);
                }
                break;

        }

        if (firewall1AnsweredQuestionCount == 3 && firewall2AnsweredQuestionCount == 3 && firewall3AnsweredQuestionCount == 3)
        {
            PlayerWin();
        }

    }

    void OnEnable()
    {
        if (unAnsweredQuestionsList == null || unAnsweredQuestionsList.Count == 0 || unAnsweredFalseQuestionsList == null || unAnsweredFalseQuestionsList.Count == 0)
        {
            unAnsweredQuestionsList = questions.ToList<Question>();
            unAnsweredFalseQuestionsList = falseQuestion.ToList<Question>();
        }

        SetCurrentQuestion();

    }

    private void OnDisable()
    {
        SetCurrentQuestion();
    }

    public void OnClicked(Button button)
    {
        buttonName = button.name;
        // gameObject.GetComponent<Minigame>().enabled = true;
        
    }

    void SetCurrentQuestion()
    {
        int randomQuestionIndex = Random.Range(0, unAnsweredQuestionsList.Count);
        int randomFalseQuestionIndex = Random.Range(0, unAnsweredFalseQuestionsList.Count);
        currentQuestion = unAnsweredQuestionsList[randomQuestionIndex];
        currentFalseQuestion = unAnsweredFalseQuestionsList[randomQuestionIndex];


        factText.text = currentQuestion.fact;
        factFalseText.text = currentFalseQuestion.fact;


    }

    IEnumerator TransitionToNextQuestion()
    {

        unAnsweredQuestionsList.Remove(currentQuestion);
        unAnsweredFalseQuestionsList.Remove(currentFalseQuestion);
        questionCanvas.gameObject.SetActive(false);
        SetCurrentQuestion();
        // this.gameObject.SetActive(false);
        // gameObject.GetComponent<Minigame>().enabled = false;

        yield return new WaitForSeconds(.1f);
    }

    public void UserSelectTrue()
    {
        if (currentQuestion.isTrue)
        {
            // Debug.Log("Correct!");
            string buttonClicked = buttonName;

            switch (buttonClicked)
            {
                case "Firewall 1 Button":
                    firewall[0].addHealth();
                    firewall1AnsweredQuestionCount++;
                    break;
                case "Firewall 2 Button":
                    firewall[1].addHealth();
                    firewall2AnsweredQuestionCount++;
                    break;
                case "Firewall 3 Button":
                    firewall[2].addHealth();
                    firewall3AnsweredQuestionCount++;
                    break;
            }
            StartCoroutine(TransitionToNextQuestion());


            // isTimerRunning = true;
        }
    }

    public void UserSelectFalse()
    {
        if (!currentFalseQuestion.isTrue)
        {
            Debug.Log("False!");
            // isTimerRunning = true;
        }
    }
    public void PlayerWin()
    {
        finishedCanvas.gameObject.SetActive(true);
        finishedText.text = "Selamat!!! kamu telah berhasil melindungi semua data yang ada di server dari serangan virus hacker Zenon.";
        // if (finishedCanvas.isActiveAndEnabled)
        // {
        //     Time.timeScale = 0f;
        // }
    }

    public void Retry()
    {
        string name = SceneManager.GetSceneByBuildIndex(SceneManager.GetActiveScene().buildIndex).name;
        SceneManager.LoadScene(name);
    }
}
