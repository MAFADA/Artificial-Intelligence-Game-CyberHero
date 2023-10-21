using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MidBossStageTrigger : MonoBehaviour
{

    public UnityEvent OnMidBossEnter;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            OnMidBossEnter.Invoke();
        }
    }
}
