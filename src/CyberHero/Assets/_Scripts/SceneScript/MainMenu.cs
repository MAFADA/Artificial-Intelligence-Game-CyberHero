using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Animator transition;
    [SerializeField] float transitionTime = 1f;

    public void StartGame()
    {
        LoadNextScene();
    }

    public void LoadAlexStageLevel()
    {
        StartCoroutine(LoadScene(SceneManager.GetSceneByName("CutsceneIntroAlex").buildIndex));
    }

    public void LoadNextScene()
    {
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void LoadMainMenuLevel()
    {
        StartCoroutine(LoadScene(0));
    }

    IEnumerator LoadScene(int levelIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSecondsRealtime(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }

    public void Restart()
    {
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex));
    }

    public void Quit()
    {
        Application.Quit();
    }
}
