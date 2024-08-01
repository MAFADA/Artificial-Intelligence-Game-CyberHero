using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingMenu;
    public GameObject gameOverPanel;

    [SerializeField] private Player player;
    [SerializeField] private PlayerInputHandler inputHandler;

    private bool isPaused;
    void Start()
    {
        mainMenu.SetActive(false);
        settingMenu.SetActive(false);
    }

    void Update()
    {
        if (InputManager.instance.MenuCloseOpenInput)
        {
            if (!isPaused)
            {
                Pause();
            }
            else
            {
                UnPause();
            }
        }
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        player.enabled = false;
        inputHandler.enabled = false;

        OpenMainMenu();
    }

    void UnPause()
    {
        isPaused = false;
        Time.timeScale = 1f;

        player.enabled = true;
        inputHandler.enabled = true;

        CloseMainMenu();
    }

    void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        settingMenu.SetActive(false);
    }

    void CloseMainMenu()
    {

        mainMenu.SetActive(false);
        settingMenu.SetActive(false);

    }
  
    public void OpenGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }

    void OpenSettingMenu()
    {
        mainMenu.SetActive(false);
        settingMenu.SetActive(true);
    }

    public void OnSettingPress()
    {
        OpenSettingMenu();
    }

    public void OnResumePresse()
    {
        UnPause();
    }

    public void OnSettingBackPress()
    {
        OpenMainMenu();
    }


}
