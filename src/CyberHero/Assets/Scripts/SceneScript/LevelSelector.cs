using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] string levels;

    public void LoadLevel()
    {
        SceneManager.LoadScene(levels);
    }
}
