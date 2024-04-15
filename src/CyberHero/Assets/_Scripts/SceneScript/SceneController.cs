using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;
    [SerializeField] private Animator anim;
    [SerializeField] Canvas transitionCanvas;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);    
        }
    }
    
    public void NextLevel()
    {
        StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel()
    {
        anim.SetTrigger("End");
        yield return new WaitForSeconds(1);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        anim.SetTrigger("Start");
    }

}
