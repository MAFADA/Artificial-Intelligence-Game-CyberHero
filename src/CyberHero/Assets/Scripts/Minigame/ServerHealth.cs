using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ServerHealth : MonoBehaviour
{
    [SerializeField] float health = 100f;
    [SerializeField] float counterAtk = 30f;
    [SerializeField] Image healhtBar;
    [SerializeField] Canvas finishedCanvas;
    [SerializeField] TMP_Text finishedText;
    public UnityEvent OnServerHit;

    private bool isGameOver;

    private void Start()
    {
        healhtBar.fillAmount = health / 100;
    }

    private void Update()
    {
        if (health <= 0)
        {

            Destroy(this.gameObject);
            GameOver();
        }

        healhtBar.fillAmount = health / 100;
    }

    public void TakeDamage(float damage)
    {
        OnServerHit.Invoke();
        health -= damage;
        healhtBar.fillAmount = health / 100;
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.tag == "Virus")
        {
            other.gameObject.GetComponent<Virus>().TakeDamage(counterAtk);
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        finishedCanvas.gameObject.SetActive(true);
        finishedText.text = "Game Over!!!";
        // if (finishedCanvas.isActiveAndEnabled)
        // {
        //     Time.timeScale = 0f;
        // }
    }

  



}
