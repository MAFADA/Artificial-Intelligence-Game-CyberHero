using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MidBossEvent : MonoBehaviour
{
    private Stats agentStat;
    private void Start()
    {
        agentStat = GetComponentInChildren<Stats>();
    }

    private void OnDisable()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
