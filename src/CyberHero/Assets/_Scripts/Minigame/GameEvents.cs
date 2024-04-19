using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="GameEvents", menuName ="Quiz/new GameEvents")]

public class GameEvents : ScriptableObject
{
    public delegate void UpdateQuestionUICallback(Question question);
    public UpdateQuestionUICallback UpdateQuestionUI;

    public delegate void UpdateQuestionAnswrcallback(AnswerData pickedAnswer);
    public UpdateQuestionAnswrcallback UpdateQuestionAnswer;

    public delegate void DisplayResolutionScreenCallback(UIController.ResolutionScreenType type, int score);
    public DisplayResolutionScreenCallback DisplayResolutionScreen;

    public delegate void ScoreUpdatedCallback();
    public ScoreUpdatedCallback ScoreUpdated;

    //[HideInInspector]
    public int CurrentFinalScore;
    [HideInInspector]
    public int StartupHighscore;
}
