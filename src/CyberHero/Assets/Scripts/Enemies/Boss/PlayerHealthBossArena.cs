using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHealthBossArena : MonoBehaviour
{
    [SerializeField] float health = 100f;
    [SerializeField] TMP_Text gameOverText;
    public UnityEvent OnDie;
    public UnityEvent OnFallFromBelowGround;
    public UnityEvent OnBossStageEnter;
    public UnityEvent<GameObject> OnHit;
    

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    private void Update() {
        if (health <= 0) {
            Destroy(this.gameObject);
            OnDie.Invoke();
        }

    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("FallTrigger"))
        {
            gameOverText.text = "Kamu jatuh ke lubang void!!!\n Berhati-hati lah";
            OnFallFromBelowGround.Invoke();
        }
        if (other.CompareTag("MidBoss1StageTrigger"))
        {
            OnBossStageEnter.Invoke();
        }
    }

}
