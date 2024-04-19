using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct Answer
{
    [SerializeField] private string info;
    public string Info { get { return info; } }

    [SerializeField] private bool isCorrect;
    public bool IsCorrect { get { return isCorrect; } }
}

[CreateAssetMenu(fileName = "New Question", menuName = "Quiz/new Question")]
public class Question : ScriptableObject
{
    public enum AnswerType { Multi, Single }

    [SerializeField] private string info = string.Empty;
    public string Info { get { return info; } }

    [SerializeField] Answer[] answers = null;
    public Answer[] Answers { get { return answers; } }

    [SerializeField] private bool useTimer = false;
    public bool UseTimer { get { return useTimer; } }

    [SerializeField] private int timer = 0;
    public int Timer { get { return timer; } }

    [SerializeField] private AnswerType answerType = AnswerType.Multi;
    public AnswerType GetAnswerType { get { return answerType; } }

    [SerializeField] private int addScore = 10;
    public int AddScore { get { return addScore; } }

    public List<int> GetCorrectAnswers()
    {
        List<int> CorrectAnswers = new();
        for (int i = 0; i < Answers.Length; i++)
        {
            if (Answers[i].IsCorrect)
            {
                CorrectAnswers.Add(i);
            }
        }
        return CorrectAnswers;
    }
}
