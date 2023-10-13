using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FallTrigger : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    public UnityEvent OnFall;

    public void GameOverPanel() {
        titleText.text ="Kamu jatuh ke area void!!!\nBerhati-hatilah";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            OnFall.Invoke();
        }
    }
}
