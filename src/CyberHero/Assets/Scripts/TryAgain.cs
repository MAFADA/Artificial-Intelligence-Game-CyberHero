using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TryAgain : MonoBehaviour
{
    public UnityEvent OnRestart;
    public void Restart()
    {
        OnRestart.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
