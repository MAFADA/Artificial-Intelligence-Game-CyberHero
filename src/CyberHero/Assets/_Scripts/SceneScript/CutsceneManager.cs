using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] string nextSceneName;
    public PlayableDirector playableDirector;
    [SerializeField] Animator transition;
    [SerializeField] float transitionTime = 1f;

    private void Start()
    {
        playableDirector.stopped += OnCutsceneFinished;
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        // SceneManager.LoadScene(nextSceneName);
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadScene(int levelIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }

}
