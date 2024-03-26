using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AsynLoader : MonoBehaviour
{
  [Header("Menu Screen")]
  [SerializeField] private GameObject loadingScreen;
  [SerializeField] private GameObject mainMenu;

  [Header("Loading Slider")]
  [SerializeField] private Slider loadingSlider;


  public void LoadLevelBtn(string levelToLoad)
  {
    mainMenu.SetActive(false);
    loadingScreen.SetActive(true);

    StartCoroutine(LoadLevelAsync(levelToLoad));
  }

  IEnumerator LoadLevelAsync(string levelToLoad)
  {
    AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
    while (!loadOperation.isDone)
    {
      float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
      loadingSlider.value = progressValue;
      yield return null;
    }
  }
}
