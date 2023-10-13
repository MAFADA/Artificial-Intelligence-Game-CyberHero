using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PickUpItem : MonoBehaviour
{
    public UnityEvent OnCollidePlayer = new UnityEvent();
    public UnityEvent OnStayPlayer = new UnityEvent();
    public UnityEvent OnLeavePlayer = new UnityEvent();

    bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnCollidePlayer.Invoke();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.F))
            {
                if (!collected)
                {
                    collected = true;
                    OnStayPlayer.Invoke();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnLeavePlayer.Invoke();
        }
    }

    private void Update()
    {
        if (collected)
        {
            Destroy(gameObject);
        }
    }
}
