using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Animator transition;
    [SerializeField] float transitionTime = 1f;
    public UnityEvent OnEscKeyDown;

    public void StartGame()
    {
        LoadNextScene();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)){
            OnEscKeyDown.Invoke();
        }
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
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator LoadScene(int levelIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneBuildIndex: levelIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
