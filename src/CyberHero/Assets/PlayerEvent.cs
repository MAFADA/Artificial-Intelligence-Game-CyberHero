using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerEvent : MonoBehaviour
{
    private Stats playerStat;
    public UnityEvent eventPlayer;

    void Awake()
    {
        playerStat = GetComponentInChildren<Stats>();
    }

    void Update()
    {
        if (playerStat.CurrentHealth <= 0f)
        {
            eventPlayer.Invoke();
        }
    }
}
