using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{
    public UnityEvent OnBackToMainMenu;

    public void MainMenuScene(){
        OnBackToMainMenu.Invoke();
        SceneManager.LoadScene("MainMenu");
    }
}
