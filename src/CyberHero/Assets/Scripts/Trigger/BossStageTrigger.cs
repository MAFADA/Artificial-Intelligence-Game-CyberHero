using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossStageTrigger : MonoBehaviour
{
    public UnityEvent OnBossEnter;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            OnBossEnter.Invoke();
        }
    }
}
